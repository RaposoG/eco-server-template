using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;

namespace Eco.Mods.StarterRewards
{
    [ChatCommandHandler]
    public static class StarterRewardsCommands
    {
        [ChatCommand("Manage starter money and starter specialty stars.", ChatAuthorizationLevel.Admin)]
        public static void StarterRewards() { }

        [ChatSubCommand("StarterRewards", "Show the current starter reward settings.", ChatAuthorizationLevel.Admin)]
        public static void Info(User user)
        {
            var p = StarterRewardsPlugin.Obj;
            if (p == null)
            {
                user.Player?.MsgLocStr("Starter Rewards is not loaded.");
                return;
            }

            user.Player?.MsgLocStr(
                $"First login only: {p.Config.OnlyOnFirstLogin}\n" +
                $"Starter money enabled: {p.Config.EnableStarterMoney}\n" +
                $"Currency: {p.Config.CurrencyName}\n" +
                $"Starting amount: {p.Config.StartingAmount}\n" +
                $"Starter stars enabled: {p.Config.EnableStarterStars}\n" +
                $"Number of stars: {p.Config.NumberOfStars}");
        }

        [ChatSubCommand("StarterRewards", "Set the starter money amount. Example: /starterrewards setmoney 500", ChatAuthorizationLevel.Admin)]
        public static void SetMoney(User user, float amount)
        {
            var p = StarterRewardsPlugin.Obj;
            if (p == null) return;
            p.Config.StartingAmount = amount;
            p.SaveConfig();
            user.Player?.MsgLocStr($"Starter money set to {p.Config.StartingAmount}.");
        }

        [ChatSubCommand("StarterRewards", "Set the number of starter specialty stars. Example: /starterrewards setstars 2", ChatAuthorizationLevel.Admin)]
        public static void SetStars(User user, int count)
        {
            var p = StarterRewardsPlugin.Obj;
            if (p == null) return;
            p.Config.NumberOfStars = count;
            p.SaveConfig();
            user.Player?.MsgLocStr($"Starter stars set to {p.Config.NumberOfStars}.");
        }

        [ChatSubCommand("StarterRewards", "Rename the starter currency. Example: /starterrewards rename Eco Credits", ChatAuthorizationLevel.Admin)]
        public static void Rename(User user, string newName)
        {
            var p = StarterRewardsPlugin.Obj;
            if (p == null) return;
            p.Config.CurrencyName = newName;
            p.SaveConfig();
            var currency = p.EnsureCurrency();
            user.Player?.MsgLocStr(currency == null
                ? "The currency could not be created or renamed. Check the server log."
                : $"Starter currency is now '{currency.Name}'.");
        }

        [ChatSubCommand("StarterRewards", "Give yourself the currently configured starter rewards for testing.", ChatAuthorizationLevel.Admin)]
        public static void GiveMe(User user)
        {
            var p = StarterRewardsPlugin.Obj;
            if (p == null) return;

            var money = p.GrantMoney(user, p.Config.StartingAmount);
            var stars = p.GrantStars(user, p.Config.NumberOfStars);
            user.Player?.MsgLocStr($"Test rewards granted. Money: {money}; Stars: {stars}.");
        }

        [ChatSubCommand("StarterRewards", "Give the configured rewards to all existing users once when the command is run.", ChatAuthorizationLevel.Admin)]
        public static void GiveAll(User user)
        {
            var p = StarterRewardsPlugin.Obj;
            if (p == null) return;

            var count = 0;
            foreach (var target in UserManager.Users)
            {
                if (target == null) continue;
                p.GrantMoney(target, p.Config.StartingAmount);
                p.GrantStars(target, p.Config.NumberOfStars);
                count++;
            }

            user.Player?.MsgLocStr($"Starter rewards were granted to {count} existing user(s).");
        }
    }
}
