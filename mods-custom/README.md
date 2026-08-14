# Custom overrides

Server-specific mod code that is written here rather than downloaded. The
entrypoint copies this over `Mods/UserCode` after the downloaded mods, so it wins
any conflict and survives `scripts/fetch-mods.sh`, which rebuilds `mods-installed/`
from scratch on every run.

Paths mirror `Mods/__core__`, because that is how the game matches a
`*.override.cs` to the file it replaces.

- `AutoGen/Tool/*Shovel.override.cs` — MaxTake per tier: 10 / 20 / 30 / 50. Copies of
  the enis-better-shovels files with only that number changed, so re-running
  fetch-mods.sh cannot revert them.
