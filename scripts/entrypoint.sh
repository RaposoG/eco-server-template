#!/bin/sh
# Container entrypoint: fix Steam, install mods, start the server.
set -e

# steamclient.so ships in /app but Steamworks looks for it in ~/.steam/sdk64.
# This is the symlink the official install.sh creates; the image never runs it.
mkdir -p /root/.steam/sdk64
ln -sf /app/steamclient.so /root/.steam/sdk64/steamclient.so

# Configs are prepared in the repo and copied over the live config on every start,
# so what the server runs is what you can read in git. Editing a config on the
# server itself is therefore temporary — it is overwritten on the next start.
if [ -d /config-src ]; then
  # The server generates its own ID and Passport on first boot and they identify
  # this server to Strange Cloud. They are deliberately empty in git — carry them
  # across the refresh instead, or the server would look like a brand new one
  # (losing its place in the server browser) every single restart.
  keep_id=''; keep_passport=''
  if [ -f /app/Configs/Network.eco ]; then
    keep_id=$(sed -n 's/.*"ID"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p' /app/Configs/Network.eco | head -1)
    keep_passport=$(sed -n 's/.*"Passport"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p' /app/Configs/Network.eco | head -1)
  fi

  for f in /config-src/*.eco /config-src/*.eco.template; do
    [ -e "$f" ] || continue
    cp "$f" "/app/Configs/$(basename "$f")"
  done

  # Insert them back rather than leaving empty placeholders in git: these fields are
  # typed as Guid, and an empty string is not a valid one — the server dies reading
  # the config with "Unrecognized Guid format" before it starts. Absent is fine.
  if [ -n "$keep_id" ]; then
    sed -i "1s|^{|{\n  \"ID\": \"$keep_id\",\n  \"Passport\": \"$keep_passport\",|" /app/Configs/Network.eco
    echo "[entrypoint] kept this server's existing identity"
  fi
fi

# Schematics shipped with the repo are seeded once. Anything you save in game
# afterwards stays in the volume and is not overwritten by a redeploy.
# EcoWorldEdit looks for schematics in StorageDirectory/Blueprints, not /app/Blueprints
# — confirmed in its GetSchematicDirectory(), which does Path.Combine on the storage
# config. That folder is inside the world volume, so what you save in game persists.
if [ -d /blueprints-src ]; then
  bp_dir=/app/Storage/Blueprints
  mkdir -p "$bp_dir"
  for bp in /blueprints-src/*.ecobp; do
    [ -e "$bp" ] || continue
    target=$bp_dir/$(basename "$bp")
    [ -f "$target" ] || cp "$bp" "$target"
  done
  echo "[entrypoint] blueprints available: $(ls "$bp_dir" 2>/dev/null | wc -l)"
fi

# Mods are copied in on every start rather than baked into a volume, so they
# survive both `--force-recreate` and an image update (the game's own content
# lives in /app/Mods/__core__ and must come from the image, not from a volume).
if [ -d /mods-installed ] && [ -n "$(ls -A /mods-installed 2>/dev/null)" ]; then
  cp -r /mods-installed/. /app/
  echo "[entrypoint] mods installed: $(ls /app/Mods/UserCode | wc -l) entries in Mods/UserCode"
fi

# The server refuses to boot without authentication; fall back to offline mode.
AUTH=${ECO_TOKEN:+-userToken=$ECO_TOKEN}
exec ./EcoServer --nogui ${AUTH:--offline}
