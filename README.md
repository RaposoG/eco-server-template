# Eco Server Template

A ready-to-run [Eco](https://store.steampowered.com/app/382310/Eco/) dedicated server:
official image, Docker Compose, named volumes. Two commands and you have a world.

No Dockerfile, no SteamCMD, no manual config copying — it uses the official
[`strangeloopgames/eco-game-server`](https://hub.docker.com/r/strangeloopgames/eco-game-server)
image and works around the two things that stop it from booting out of the box
(see [Why it doesn't just work](#why-it-doesnt-just-work)).

## Requirements

- Docker Engine with Compose v2 (`docker compose version`)
- ~4 GB RAM for a small world, more for bigger ones
- Linux, macOS, or Windows (Docker Desktop / WSL2)

## Quick start

```bash
cp .env.example .env
docker compose up -d
```

First boot generates the world — around 4–5 minutes on a modest machine. Later
starts load the saved world in seconds. Follow along:

```bash
docker compose logs -f
```

Then:

- **Join the game:** in Eco, `Join Game` → `Direct Connect` → `localhost:3000`
  (or `YOUR_IP:3000` from another machine)
- **Web UI / admin:** <http://localhost:3001>

## Configuration

Everything is driven by `.env`:

| Variable      | Default  | What it does                                                       |
|---------------|----------|--------------------------------------------------------------------|
| `ECO_VERSION` | `latest` | Image tag. Pin it (e.g. `0.14.0.2-beta`) to control when you update. |
| `ECO_TOKEN`   | empty    | Your Eco user token — see below.                                    |
| `GAME_PORT`   | `3000`   | Host port for the game (UDP).                                       |
| `WEB_PORT`    | `3001`   | Host port for the web UI (TCP).                                     |

### Authentication

The server **refuses to start** without authentication, so this template falls
back to offline mode when `ECO_TOKEN` is empty. Offline works for a private
server, but it is not listed publicly and **cannot download mods from mod.io**.

For everything else, grab your token at <https://play.eco/account> and put the
value alone in `.env` — no flag, no quotes:

```ini
ECO_TOKEN=your_token_here
```

`.env` is gitignored, and the token reaches the container as an environment
variable rather than a command-line argument, so it stays out of `docker inspect`
and `ps` output.

### Game settings

Game settings are plain JSON in `./Configs`, versioned in git. You prepare them
where you develop, commit, and the deploy runs exactly those files: the container
copies `./Configs` over its live config on every start.

```bash
$EDITOR Configs/Difficulty.eco
docker compose up -d --force-recreate
```

The live config itself lives in a **named volume**, never a host folder. PaaS
tooling wipes the host folder on each deploy, so anything that has to survive one
— the world, the backups, the server's identity — belongs in a volume. The host
folder holds only what git already has.

**Recreate the container, do not just restart it.** A deploy that re-clones or runs
`git reset --hard` replaces the `Configs` directory, so it gets a new inode while the
running container's bind mount still points at the old one. The mount stays listed and
looks fine, `/config-src` is simply empty, and the server quietly keeps running the
config already in its volume — no error anywhere. `docker compose up -d
--force-recreate` reattaches it.

The consequence worth knowing: editing a config **on the server** is temporary,
because the next start copies the repo version back over it. Settings you want to
change per-machine without a commit go in `.env` instead — that is what
`GAME_PORT` and `WEB_PORT` are for.

The one exception is the server's `ID` and `Passport` in `Network.eco`. They are
committed empty on purpose: the server generates them on first boot, and the
entrypoint carries them across each config refresh. Without that the server would
look like a brand new one to the server browser after every restart.

`Network.eco` holds the server name, description, password and ports.
`Difficulty.eco` holds the meteor timer and every progression multiplier.
`WorldGenerator.eco` controls world size and terrain, and only takes effect on a
fresh world. Some `Difficulty.eco` values are read at every start and others are
fixed when the world is created, so change them before generating a world you
intend to keep.

The `.eco.template` files next to them are the server's own defaults: useful to
diff against when you want to know what you changed.

This template comes tuned rather than vanilla — meteor at 90 days, halved craft
resources, craft time and skill cost, 5× stack sizes, doubled growth rate, fuel
efficiency and shelf life, halved item weight, exhaustion off, and full
specialty refunds. Reset any of it by copying the value back from the matching
`.eco.template`.

### Admin access and the server UI

Eco's native server GUI is **Windows only** — a Linux server has no window, which
is why the container runs with `--nogui`. The equivalent lives in the game: as an
admin, type `/serverui` in chat to open the server configuration UI, and
`/help` for the rest of the admin commands.

To make yourself an admin, add your Steam ID to `Users.eco`:

```bash
docker compose cp eco:/app/Configs/Users.eco ./Users.eco
```

Find `UserPermission` → `Admins` → `$values` (an empty list) and put your ID in it:

```json
"$values": ["76561198000000000"]
```

Then push it back and restart:

```bash
docker compose cp ./Users.eco eco:/app/Configs/Users.eco
docker compose restart
```

The web UI on port 3001 is the **World UI** — map layers, ecosystem graphs,
elections and officials. It is not a configuration panel.

### Mods

The server does **not** download mods from mod.io — `SubscribedMods` in
`ModKit.eco` only tells connecting clients what to fetch. Server-side, a mod is
files in `Mods/`, so this template downloads and installs them for you:

```bash
scripts/fetch-mods.sh
docker compose up -d --force-recreate
```

It reads the mod IDs from `Configs/ModKit.eco`, pulls each archive from mod.io
into `./mods`, and lays the files out under `./mods-installed`, which the
container copies into `/app` on every start. That survives `--force-recreate` and
image updates, unlike installing into a volume.

To add a mod, put its numeric ID in `Configs/ModKit.eco` — it is the number in
the mod's image URLs on its mod.io page — and re-run the script. `MODIO_API_KEY`
in `.env` is only used to resolve each mod's current file; the download URL that
comes back is pre-signed.

Eco mods are packaged inconsistently, so the script also does what a human
otherwise does by hand:

- picks the right extraction root per archive (three layouts are in use)
- relocates whole-file `.override.cs` replacements onto the path the game
  matches them against, while leaving `ModsPreInitialize` partials alone
- keeps one language folder per mod (`MODS_LANG`, default `English`) — mods that
  ship several compile all of them and collide
- skips `.ecobp` blueprints, which are not server mods

Two curation files carry the decisions that cannot be automated, each entry with
the compiler error that justifies it:

- `mods-excluded.txt` — mods dropped entirely, mostly built against older Eco APIs
- `mods-remove.txt` — single broken files inside mods that are otherwise fine

A mod that fails to compile inside `Mods/UserCode` is survivable; one that breaks
a file in `Mods/__core__` kills the server on startup. Check `docker compose logs`
for `error CS` after adding anything.

### Ports

| Port | Proto | Purpose                                            |
|------|-------|----------------------------------------------------|
| 3000 | UDP   | Game traffic — required                             |
| 3001 | TCP   | Web UI and API — required for the in-game web pages |
| 3002 | TCP   | RCON — commented out; set `RconPassword` first      |
| 3003 | UDP   | Steam query — uncomment for the Steam server browser |

Uncomment the last two in `docker-compose.yml` if you need them. Forward the
same ports on your router to play over the internet.

## Data, backups, updates

World saves live in the `eco-storage` volume, configs in `eco-configs`. Both
survive `docker compose down`. Only `docker compose down -v` destroys them.

Back up (stop first so the save is consistent):

```bash
docker compose stop
docker run --rm -v eco-server_eco-storage:/from -v "$PWD:/to" alpine \
  tar czf /to/eco-storage-backup.tar.gz -C /from .
docker compose start
```

Restore:

```bash
docker compose down
docker volume rm eco-server_eco-storage
docker volume create eco-server_eco-storage
docker run --rm -v eco-server_eco-storage:/to -v "$PWD:/from" alpine \
  tar xzf /from/eco-storage-backup.tar.gz -C /to
docker compose up -d
```

Update the server:

```bash
docker compose pull && docker compose up -d
```

Server and client versions must match. If you pin `ECO_VERSION`, bump it when
Steam updates the game; if you leave it on `latest`, back up before pulling.

## Why it doesn't just work

Two things bite everyone running the official image, and both are handled in
`docker-compose.yml`:

1. **Authentication is mandatory.** The image's default command is
   `./EcoServer --nogui`, and the server exits with *"Authentication to Strange
   Cloud failed"* unless you add `-offline`, `-userToken`, or `-username`/`-password`.
2. **Steam can't find `steamclient.so`.** The file ships in `/app`, but
   Steamworks looks in `~/.steam/sdk64/`, so the server dies with *"Steam game
   server failed to initialize"*. The official `install.sh` creates that symlink;
   the image never runs it. The compose command creates it before starting.

One more trap, avoided here: **don't mount a volume at `/app/Mods`.** That
directory holds the game's own content, so an empty volume wipes the server.
Mount `/app/Mods/UserCode` instead if you want mods.

## Commands

```bash
docker compose up -d          # start
docker compose logs -f        # follow logs
docker compose restart        # restart (after a config change)
docker compose stop           # stop, keeping the world
docker compose down           # remove the container, keeping the volumes
docker compose down -v        # remove everything, world included
```

## License

MIT — see [LICENSE](LICENSE).
