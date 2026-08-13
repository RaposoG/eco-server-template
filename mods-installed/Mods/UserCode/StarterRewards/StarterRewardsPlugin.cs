using System;
using System.Linq;
using Eco.Core.Plugins;
using Eco.Core.Plugins.Interfaces;
using Eco.Core.Utils;
using Eco.Gameplay.Economy;
using Eco.Gameplay.Players;
using Eco.Shared.Items;
using Eco.Shared.Logging;
using Eco.Shared.Utils;

namespace Eco.Mods.StarterRewards
{
    public class StarterRewardsPlugin : IModKitPlugin, IServerPlugin, IInitializablePlugin, IConfigurablePlugin
    {
        public static StarterRewardsPlugin Obj { get; private set; }

        private readonly PluginConfig<StarterRewardsConfig> config =
            new PluginConfig<StarterRewardsConfig>("StarterRewards");

        public IPluginConfig PluginConfig => this.config;
        public StarterRewardsConfig Config => this.config.Config;
        public Currency StarterCurrency { get; private set; }

        public string GetCategory() => "Mods";

        public string GetStatus() =>
            $"Active - Money: {(this.Config.EnableStarterMoney ? this.Config.StartingAmount.ToString() : "Disabled")}, " +
            $"Stars: {(this.Config.EnableStarterStars ? this.Config.NumberOfStars.ToString() : "Disabled")}, " +
            $"First login only: {this.Config.OnlyOnFirstLogin}";

        public object GetEditObject() => this.Config;

        public void OnEditObjectChanged(object o, string param)
        {
            try
            {
                this.NormalizeConfig();
                this.config.SaveAsync();
                if (this.Config.EnableStarterMoney) this.EnsureCurrency();
            }
            catch (Exception ex)
            {
                Log.WriteErrorLineLocStr($"[StarterRewards] Failed to save configuration: {ex.Message}");
            }
        }

        public ThreadSafeAction<object, string> ParamChanged { get; set; } =
            new ThreadSafeAction<object, string>();

        public void Initialize(TimedTask timer)
        {
            Obj = this;
            this.NormalizeConfig();
            UserManager.OnUserLoggedIn.Add(this.OnUserLoggedIn);
            if (this.Config.EnableStarterMoney) this.EnsureCurrency();
        }

        private void NormalizeConfig()
        {
            this.Config.StartingAmount = Math.Max(0f, this.Config.StartingAmount);
            this.Config.NumberOfStars = Math.Max(0, this.Config.NumberOfStars);
            if (string.IsNullOrWhiteSpace(this.Config.CurrencyName)) this.Config.CurrencyName = "Eco Credits";
            if (this.Config.WelcomeMessage == null) this.Config.WelcomeMessage = string.Empty;
        }

        public Currency EnsureCurrency()
        {
            var desired = this.Config.CurrencyName?.Trim();
            if (string.IsNullOrEmpty(desired)) desired = "Eco Credits";

            if (this.StarterCurrency != null && this.StarterCurrency.Name == desired)
                return this.StarterCurrency;

            var existing = CurrencyManager.Currencies.FirstOrDefault(c => c.Name == desired);
            if (existing != null)
            {
                this.StarterCurrency = existing;
                return existing;
            }

            if (this.StarterCurrency != null)
            {
                this.StarterCurrency.Name = desired;
                return this.StarterCurrency;
            }

            var owner = UserManager.Admins?.FirstOrDefault() ?? UserManager.Users?.FirstOrDefault();
            try
            {
                this.StarterCurrency = CurrencyManager.AddCurrency(owner, desired, CurrencyType.Credit);
                return this.StarterCurrency;
            }
            catch (Exception ex)
            {
                Log.WriteErrorLineLocStr($"[StarterRewards] Could not create currency: {ex.Message}");
                return null;
            }
        }

        private void OnUserLoggedIn(User user)
        {
            if (user == null) return;
            if (this.Config.OnlyOnFirstLogin && !user.FirstLogin) return;

            try
            {
                var moneyGranted = this.GrantMoney(user, this.Config.StartingAmount);
                var starsGranted = this.GrantStars(user, this.Config.NumberOfStars);

                if (!moneyGranted && !starsGranted) return;

                var currencyName = this.StarterCurrency?.Name ?? this.Config.CurrencyName;
                var message = this.Config.WelcomeMessage
                    .Replace("{money}", moneyGranted ? this.Config.StartingAmount.ToString() : "0")
                    .Replace("{currency}", currencyName ?? string.Empty)
                    .Replace("{stars}", starsGranted ? this.Config.NumberOfStars.ToString() : "0");

                if (!string.IsNullOrWhiteSpace(message))
                    user.Player?.MsgLocStr(message);
            }
            catch (Exception ex)
            {
                Log.WriteErrorLineLocStr($"[StarterRewards] Failed to grant rewards: {ex.Message}");
            }
        }

        public bool GrantMoney(User user, float amount)
        {
            if (!this.Config.EnableStarterMoney || user?.BankAccount == null || amount <= 0f) return false;

            var currency = this.EnsureCurrency();
            if (currency == null) return false;

            user.BankAccount.AddCurrency(currency, amount);
            return true;
        }

        public bool GrantStars(User user, int count)
        {
            if (!this.Config.EnableStarterStars || user?.UserXP == null || count <= 0) return false;

            for (var i = 0; i < count; i++)
                user.UserXP.AddExperience(1 + (user.UserXP.NextStarCost - user.UserXP.XP));

            return true;
        }

        public void SaveConfig()
        {
            this.NormalizeConfig();
            this.config.SaveAsync();
        }

        public void SaveData(object o, string param) { }
        public override string ToString() => "Starter Rewards";
    }
}
