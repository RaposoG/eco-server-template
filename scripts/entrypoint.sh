#!/bin/sh
# Container entrypoint: fix Steam, install mods, start the server.
set -e

# steamclient.so ships in /app but Steamworks looks for it in ~/.steam/sdk64.
# This is the symlink the official install.sh creates; the image never runs it.
mkdir -p /root/.steam/sdk64
ln -sf /app/steamclient.so /root/.steam/sdk64/steamclient.so

# A few configs accumulate per-server state — Network.eco gets this server's ID and
# Passport, Users.eco collects player IDs — so they are gitignored and shipped as
# .example instead. Seed them on a fresh checkout rather than making the operator
# remember to copy them.
for example in /app/Configs/*.eco.example; do
  [ -e "$example" ] || continue
  target=${example%.example}
  if [ ! -f "$target" ]; then
    cp "$example" "$target"
    echo "[entrypoint] created Configs/$(basename "$target") from the example"
  fi
done

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
