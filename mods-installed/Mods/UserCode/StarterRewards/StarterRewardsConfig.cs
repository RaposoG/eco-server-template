using System.ComponentModel;

namespace Eco.Mods.StarterRewards
{
    public class StarterRewardsConfig
    {
        [Description("Give the configured rewards only on a player's first login. If false, rewards are given on every login.")]
        public bool OnlyOnFirstLogin { get; set; } = true;

        [Description("Enable or disable starter money.")]
        public bool EnableStarterMoney { get; set; } = true;

        [Description("Name of the starter currency. The plugin will use an existing currency with this name or create one if needed.")]
        public string CurrencyName { get; set; } = "Eco Credits";

        [Description("Amount of starter money given to each eligible player.")]
        public float StartingAmount { get; set; } = 500f;

        [Description("Enable or disable starter specialty stars.")]
        public bool EnableStarterStars { get; set; } = true;

        [Description("Number of specialty stars given to each eligible player.")]
        public int NumberOfStars { get; set; } = 2;

        [Description("Message shown after rewards are granted. Use {money}, {currency}, and {stars} as placeholders.")]
        public string WelcomeMessage { get; set; } = "<color=green>Welcome!</color> You received {money} {currency} and {stars} specialty star(s).";
    }
}
