#!/usr/bin/env bash
# Download every mod listed in Configs/ModKit.eco from mod.io and lay the files
# out the way the server expects them, under ./mods-installed.
#
# Needs MODIO_API_KEY in .env — create a read-only key at https://mod.io/me/access
# The key is only used to resolve each mod's current file; the download URL it
# returns is pre-signed and needs no credentials.
#
# ./mods           downloaded .zip archives (cache — safe to delete)
# ./mods-installed the tree that gets copied into /app inside the container
set -euo pipefail

cd "$(dirname "$0")/.."

if [ -f .env ]; then set -a; . ./.env; set +a; fi
: "${MODIO_API_KEY:?missing MODIO_API_KEY — add it to .env (https://mod.io/me/access)}"

command -v unzip >/dev/null || { echo "unzip is required"; exit 1; }

mkdir -p mods

# Rebuild the install tree from scratch every run. Extracting over the previous one
# leaves files behind from mods you have since excluded, so the exclusion appears to
# work while the mod is still loaded. Everything here comes from the cached archives
# in ./mods, so this costs nothing.
rm -rf mods-installed
mkdir -p mods-installed

ids=$(grep -oE '^[[:space:]]*[0-9]{4,},?$' Configs/ModKit.eco | tr -d ' ,')
total=$(printf '%s\n' "$ids" | wc -l | tr -d ' ')
ok=0; fail=0; skip=0; n=0

excluded=''
[ -f mods-excluded.txt ] && excluded=$(grep -oE '^[0-9]+' mods-excluded.txt || true)

for id in $ids; do
  n=$((n + 1))

  if printf '%s\n' "$excluded" | grep -qx "$id"; then
    echo "[$n/$total] $id — excluded (see mods-excluded.txt)"; skip=$((skip + 1)); continue
  fi
  meta=$(curl -sf --max-time 30 "https://api.mod.io/v1/games/@eco/mods/$id?api_key=$MODIO_API_KEY") || {
    echo "[$n/$total] $id — API request failed"; fail=$((fail + 1)); continue
  }

  # Cut to the modfile object first: the mod's logo/icon carry a "filename" too,
  # and a naive grep over the whole payload happily downloads the logo instead.
  modfile=${meta#*\"modfile\":}
  file=$(printf '%s' "$modfile" | grep -oE '"filename":"[^"]+"' | head -1 | cut -d'"' -f4)
  url=$(printf '%s' "$modfile" | grep -oE '"binary_url":"[^"]+"' | head -1 | cut -d'"' -f4 | sed 's#\\/#/#g')

  if [ -z "$file" ] || [ -z "$url" ]; then
    echo "[$n/$total] $id — no downloadable file"; fail=$((fail + 1)); continue
  fi

  if [ ! -s "mods/$file" ]; then
    if ! curl -sfL --max-time 600 -o "mods/$file.part" "$url"; then
      rm -f "mods/$file.part"
      echo "[$n/$total] $file — download failed"; fail=$((fail + 1)); continue
    fi
    # A truncated download or an error page is still a file; a zip starts with "PK".
    if [ "$(head -c 2 "mods/$file.part")" != "PK" ]; then
      rm -f "mods/$file.part"
      echo "[$n/$total] $file — not a zip, discarded"; fail=$((fail + 1)); continue
    fi
    mv "mods/$file.part" "mods/$file"
  fi

  # Eco mods are packaged three different ways, and picking the wrong root either
  # buries the mod a level too deep or scatters it over the server root.
  entries=$(unzip -Z1 "mods/$file")
  if ! printf '%s\n' "$entries" | grep -qvE '\.ecobp$'; then
    echo "[$n/$total] $file — blueprint (.ecobp), not a server mod, skipped"
    skip=$((skip + 1)); continue
  elif printf '%s\n' "$entries" | grep -qE '^Mods/'; then
    dest=mods-installed                    # archive is rooted at the server dir
  elif printf '%s\n' "$entries" | grep -qE '^UserCode/'; then
    dest=mods-installed/Mods               # archive is rooted at Mods/
  else
    dest=mods-installed/Mods/UserCode      # bare mod folder
  fi

  mkdir -p "$dest"
  # unzip exits 1 on warnings (some archives carry non-UTF-8 filenames); only
  # exit codes of 2 and up are real failures.
  unzip -qo "mods/$file" -d "$dest" || [ $? -le 1 ]
  echo "[$n/$total] $file -> ${dest#mods-installed/}"
  ok=$((ok + 1))
done

# Eco matches "X.override.cs" against __core__/<same relative path>/X.cs. A mod that
# nests its override inside its own folder never matches, and the file then compiles
# *alongside* the original instead of replacing it — every member collides. Put them
# back where the game looks for them.
core=$(docker run --rm --entrypoint sh "strangeloopgames/eco-game-server:${ECO_VERSION:-latest}" \
  -c 'cd /app/Mods/__core__ && find . -name "*.cs"' 2>/dev/null | sed 's|^\./||') || core=''

if [ -n "$core" ]; then
  find mods-installed/Mods/UserCode -name '*.override.cs' | while read -r f; do
    rel=${f#mods-installed/Mods/UserCode/}

    # Two different things are spelled ".override.cs". A file that implements the
    # ModsPreInitialize hook only *extends* the core class as a partial, and must
    # stay in the mod's own folder — moving it onto the core path makes it replace
    # the original, taking the partial method's declaration with it. Only whole-file
    # replacements get relocated.
    grep -q 'ModsPreInitialize' "$f" && continue

    base=${rel##*/}; base=${base%.override.cs}
    matches=$(printf '%s\n' "$core" | grep -E "(^|/)${base}\.cs$" || true)
    count=$(printf '%s\n' "$matches" | grep -c . || true)

    # Already sitting at a path the game recognises — including mods that ship the
    # same override at two valid paths, which is why this compares against every
    # core file of that name rather than picking one.
    printf '%s\n' "$matches" | grep -qxF "${rel%.override.cs}.cs" && continue

    if [ "$count" -eq 1 ]; then
      target="mods-installed/Mods/UserCode/${matches%.cs}.override.cs"
      [ "$f" = "$target" ] && continue
      mkdir -p "$(dirname "$target")"
      mv "$f" "$target"
      echo "  moved $rel -> ${target#mods-installed/Mods/UserCode/}"
    elif [ "$count" -gt 1 ]; then
      echo "  !! $rel — $count core files named $base.cs, cannot place it automatically"
    fi
  done
fi

# Several mods ship one folder per language and the server compiles all of them,
# which collides on duplicate class names. Keep a single language.
LANGS='English French Deutsch German Russian Spanish Italian Portuguese Brazilian Chinese Japanese Korean Polish Dutch Ukrainian Czech'
dropped=0
find mods-installed -type d | while read -r d; do
  base=${d##*/}
  case "$base" in *_*) ;; *) continue ;; esac
  case " $LANGS " in
    *" ${base##*_} "*)
      if [ "${base##*_}" != "${MODS_LANG:-English}" ]; then
        rm -rf "$d"
        echo "  dropped $base (keeping ${MODS_LANG:-English})"
        dropped=$((dropped + 1))
      fi
      ;;
  esac
done

# EcoWorldEdit names a chat command "export", and Eco 0.14 has one of its own now.
# ChatCommandService keys commands in a dictionary, so the duplicate throws before the
# server finishes starting. The mod ships compiled and its public source stopped at Eco
# 0.8 in 2019, so the name is renamed in place. Command names live in the attribute blob
# as length-prefixed UTF-8 (\x06export), and the replacement must be exactly as long —
# a different length would need the prefix changed and every later offset shifted.
we_dll=mods-installed/Mods/UserCode/EcoWorldEdit/EcoWorldEdit.dll
if [ -f "$we_dll" ]; then
  before=$(wc -c < "$we_dll")
  perl -0777 -pi -e 's/\x06export/\x06bpsave/' "$we_dll"
  after=$(wc -c < "$we_dll")
  if [ "$before" != "$after" ]; then
    echo "  !! EcoWorldEdit patch changed the file size ($before -> $after), removing it"
    rm -rf "$(dirname "$we_dll")"
  else
    echo "  patched EcoWorldEdit: /export -> /bpsave"
  fi
fi

if [ -f mods-remove.txt ]; then
  grep -vE '^[[:space:]]*(#|$)' mods-remove.txt | while read -r path; do
    [ -e "mods-installed/Mods/UserCode/$path" ] || continue
    rm -rf "mods-installed/Mods/UserCode/$path"
    echo "  removed $path"
  done
fi

if [ -f mods-unlock-skills.txt ]; then
  grep -vE '^[[:space:]]*(#|$)' mods-unlock-skills.txt | while read -r path; do
    target="mods-installed/Mods/UserCode/$path"
    [ -e "$target" ] || continue
    files=$(grep -rlE '^[[:space:]]*\[RequiresSkill\(' "$target" 2>/dev/null || true)
    [ -z "$files" ] && continue
    printf '%s\n' "$files" | while read -r f; do
      sed -i -E '/^[[:space:]]*\[RequiresSkill\(/d' "$f"
    done
    echo "  unlocked $path ($(printf '%s\n' "$files" | grep -c .) files)"
  done
fi

# Eco renames item tags between versions. A mod published before a rename still
# references the old name as a string, and the server aborts before loading the
# world with "Tag 'X' not found" — a runtime failure, so nothing in the build
# output warns you first. Type references like typeof(AdvancedUpgradeItem) are a
# different thing and still resolve; only the quoted tag names need rewriting.
TAG_RENAMES='AdvancedUpgrade:AdvancedModule'
for pair in $TAG_RENAMES; do
  old=${pair%%:*}; new=${pair##*:}
  grep -rl "\"$old\"" mods-installed/ 2>/dev/null | while read -r f; do
    sed -i "s/\"$old\"/\"$new\"/g" "$f"
    echo "  retagged $old -> $new in ${f#mods-installed/Mods/UserCode/}"
  done
done

echo
echo "done: $ok installed, $skip skipped, $fail failed"
echo "restart the server to load them:  docker compose up -d --force-recreate"
