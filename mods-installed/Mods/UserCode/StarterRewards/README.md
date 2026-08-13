# Starter Rewards for ECO 0.13

Combines starter money and starter specialty stars into one UserCode mod.

## Installation on PingPerfect

1. Stop the ECO server.
2. Open the PingPerfect File Manager.
3. Open `Mods/UserCode/`.
4. Upload the entire `StarterRewards` folder into `Mods/UserCode/`.
5. Remove the old `StarterMoney` and `StarterStars` folders so the rewards are not granted twice.
6. Start the server.
7. Check the startup log for UserCode compile errors.

The final path should be:

`Mods/UserCode/StarterRewards/StarterRewardsPlugin.cs`

## Configuration

After the first successful server start, ECO creates:

`Configs/StarterRewards.eco`

The configuration includes:

- `OnlyOnFirstLogin`
- `EnableStarterMoney`
- `CurrencyName`
- `StartingAmount`
- `EnableStarterStars`
- `NumberOfStars`
- `WelcomeMessage`

The plugin can also be edited through the server's Plugins interface.

## Admin commands

- `/starterrewards info`
- `/starterrewards setmoney 500`
- `/starterrewards setstars 2`
- `/starterrewards rename Eco Credits`
- `/starterrewards giveme`
- `/starterrewards giveall`

## Important

Do not run this together with the separate Starter Money or Starter Stars mods. Doing so may give new players duplicate money or stars.

This package combines the working login, currency, and specialty-star logic from the supplied ECO 0.13 UserCode mods. Test it on a backup or test server before using it on a live world.
