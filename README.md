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

| Variable      | Default    | What it does                                                     |
|---------------|------------|------------------------------------------------------------------|
| `ECO_VERSION` | `latest`   | Image tag. Pin it (e.g. `0.14.0.2-beta`) to control when you update. |
| `ECO_AUTH`    | `-offline` | Authentication mode — see below.                                  |
| `GAME_PORT`   | `3000`     | Host port for the game (UDP).                                     |
| `WEB_PORT`    | `3001`     | Host port for the web UI (TCP).                                   |

### Authentication

The server **refuses to start** without one of these. `-offline` is the default
because it needs no account, but an offline server never shows up in the public
server browser.

```ini
ECO_AUTH=-offline                          # no account, not publicly listed
ECO_AUTH=-userToken=YOUR_TOKEN             # token from https://play.eco/account
ECO_AUTH=-username=USER -password=SECRET   # alternative to the token
```

`.env` is gitignored — keep your token there, not in `docker-compose.yml`.

### Game settings

Game settings live in the `eco-configs` volume as `.eco` files (server name,
password, world size, difficulty, ...). The image seeds it with `.eco.template`
files on first start; the server writes the real `.eco` files next to them.

Edit one:

```bash
docker compose cp eco:/app/Configs/Network.eco ./Network.eco
# edit Network.eco — e.g. "Name" and "Password"
docker compose cp ./Network.eco eco:/app/Configs/Network.eco
docker compose restart
```

`Network.eco` holds the server name, password and ports. `Difficulty.eco`,
`WorldGenerator.eco` and `Features.eco` cover most of the rest. Changing world
generation only takes effect on a fresh world.

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
