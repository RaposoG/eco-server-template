# mods-fix — mods ported to Eco 0.14, staged but not installed

Each folder here is a mod from `mods-excluded.txt` that no longer compiled on Eco
0.14 and has been ported. Every one was compile-tested in a throwaway container
against the real server image, first on its own and then alongside the full mod
set: 27 mods, zero compiler errors, server reaching `Server Initialization ...
Finished`. None of them is installed — `scripts/fetch-mods.sh` still skips these
IDs. They live here until the server owner decides to enable them.

`mods-excluded.txt` records what was broken in each and what the fix was.

## Enabling one

1. Delete the mod's line from `mods-excluded.txt`.
2. Restore its asset bundle: `.unity3d` files are unmodified copies of the mod.io
   download and are not tracked (they are tens of megabytes of client-side
   models). Re-extract the mod's zip and copy the `.unity3d` back into the same
   folder. Mods with no `.unity3d` need nothing.
3. Copy the folder's contents over `mods-installed/` and restart the server.

## Before enabling, know this

- **hotwheels, elixr, gaspump, solar, tuning** ship their own 3D models. A player
  who has not installed the mod locally may see broken or missing objects.
- **tuning** is the one to be careful with. Its overrides are empty stubs that
  delete the game's own carts and trucks so its precompiled DLL can register
  replacements. It loads and starts clean, but actually placing and driving a
  vehicle was never tested — that needs a game client, not a server log.
- **currency** must ship with `NewUsersGift: 0` unless `StarterRewards`'
  `StartingAmount` is lowered to match, or new players will receive both.
