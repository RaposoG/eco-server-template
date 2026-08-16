using Eco.Core.Plugins;
using Eco.Core.Plugins.Interfaces;
using Eco.Core.Utils;
using Eco.Gameplay.Players;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Gameplay.Economy;
using System.Linq;
using Eco.Gameplay.Items;

namespace Eco.Mods.TechTree
{
    [Localized]
    public class GlobalCurrencyConfig : Singleton<GlobalCurrencyConfig>
    {
        [LocDescription("Name of the global currency used on the server.")]
        public string? GlobalCurrencyName { get; set; }

        [LocDescription("Welcome gift in global currency to give new users joining. 0 for none.")]
        public int NewUsersGift { get; set; }

        [LocDescription("Title for message window when welcoming a new user. Empty for no welcome window.")]
        public string? MessageWelcomeTitle { get; set; }

        [LocDescription("Body for message window when welcoming a new user. Empty for no welcome window.")]
        public string? MessageWelcomeBody { get; set; }
    }

    [ChatCommandHandler]
    public class GlobalCurrency : IModKitPlugin, IInitializablePlugin, IConfigurablePlugin, IDisplayablePlugin
    {
        private static PluginConfig<GlobalCurrencyConfig>? config;

        private string status = "";

        public IPluginConfig? PluginConfig => config;

        public ThreadSafeAction<object, string> ParamChanged { get; set; } = new ThreadSafeAction<object, string>();

        public void Initialize(TimedTask timer)
        {
            this.status = "Reading config";
            config = new PluginConfig<GlobalCurrencyConfig>("GlobalCurrency");
            this.status = "";

            // UserManager.OnUserLoggedIn.Add(OnUserLogin);
            UserManager.NewUserJoinedEvent.Add(OnUserLogin);
        }

        public override string ToString() => Localizer.DoStr("Global Currency");
        public string GetDisplayText() => this.status;
        public string GetStatus() => this.status;

        public string GetCategory() => "Global Currency";

        public object? GetEditObject() => config?.Config;

        public void OnEditObjectChanged(object o, string param)
        {
        }

        // Currency.Name carries an icon tag, so an exact == never matches an existing currency
        // and the server would end up with a second currency owned by the same player.
        private static Currency? FindCurrency(string? name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            return CurrencyManager.Currencies.FirstOrDefault(
                c => c != null && (c.Name == name || c.Name.Contains(name, System.StringComparison.OrdinalIgnoreCase)), null);
        }

        [ChatCommand("Create the global currency and treasury if it doesnt already exist", "gc-create", ChatAuthorizationLevel.Admin)]
        public static void Create(User user)
        {
            if (config == null)
            {
                user.TempServerMessage(Localizer.DoStr("[GlobalCurrency] Unable to locate config"));
                return;
            }

            Currency? currency = FindCurrency(config.Config.GlobalCurrencyName);

            if (currency == null)
            {
                // Create default currenct with user in scope as owner (first user logging in should be admin anyway)
                currency = CurrencyManager.AddCurrency(user, config.Config.GlobalCurrencyName, Shared.Items.CurrencyType.Backed);

                user.TempServerMessage(Localizer.DoStr("[GlobalCurrency] Global currency not found. Created it."));
            }

            // Fix issue with currency not having a backed item.
            if (currency.BackingItem == null)
            {
                currency.BackingItem = new ClaimPaperItem();
                currency.SaveInRegistrar();
            }

            string globalAccountName = config.Config.GlobalCurrencyName + " - Treasury";

            BankAccount? treasuryBankAccount = BankAccountManager.Obj.Accounts.FirstOrDefault((account) => account != null && account.Name == globalAccountName, null);

            if (treasuryBankAccount == null)
            {
                BankAccountManager.CreateAccount(user, globalAccountName);
                treasuryBankAccount = BankAccountManager.Obj.Accounts.FirstOrDefault((account) => account != null && account.Name == globalAccountName, null);

                if (treasuryBankAccount != null)
                {
                    BankAccountManager.AddAccountManager(user, treasuryBankAccount, user);
                    treasuryBankAccount.AddCurrency(currency, 1000000f);
                    user.TempServerMessage(Localizer.DoStr("[GlobalCurrency] Global currency treasury not found. Created it."));
                }
            }
        }

        public void OnUserLogin(User user)
        {
            // Check and ensure that we should give users a gift and that we have defined a currency
            if (config != null && config.Config.NewUsersGift > 0 && config.Config.GlobalCurrencyName != null && config.Config.GlobalCurrencyName != "")
            {
                // Locate the global currency in the currency list
                Currency? currency = FindCurrency(config.Config.GlobalCurrencyName);

                if (currency != null) {
                    BankAccountManager.Obj.SpawnMoney(currency, user, (float)config.Config.NewUsersGift);

                    // Should we show information box to the user about the welcome gift?
                    if (user.Player != null && config.Config.MessageWelcomeTitle != "" && config.Config.MessageWelcomeBody != null)
                    {
                        user.Player.OpenInfoPanel(config.Config.MessageWelcomeTitle, config.Config.MessageWelcomeBody.Replace("$Amount", config.Config.NewUsersGift.ToString()), "EconomyManager");
                    }
                }
            }
        }
    }
}