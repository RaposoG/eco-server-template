using System.Linq;
using Eco.Gameplay.Economy;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;

namespace Eco.Mods.BbcBrasil
{
    // Eco has no admin command that credits a single account: the Money group only
    // manages accounts, and /gabe pays every player at once. This does the missing
    // piece through BankAccount.AddCurrency, the same call StarterRewards and the Mint
    // use internally.
    //
    // The User and Currency parameters are typed rather than strings on purpose: Eco
    // resolves those itself, by name or by the ID in the tooltip. Matching a currency
    // by string fails, because the name a player reads in chat carries an icon and is
    // not the raw value compared here.
    [ChatCommandHandler]
    public static class MoedaCommands
    {
        [ChatCommand("Ferramentas de moeda do servidor.", ChatAuthorizationLevel.Admin)]
        public static void Moeda() { }

        [ChatSubCommand("Moeda", "Credita moeda na conta de um jogador. Ex: /moeda dar Raposo 10000 Real", ChatAuthorizationLevel.Admin)]
        public static void Dar(User user, User jogador, float quantia, Currency moeda)
        {
            if (jogador?.BankAccount == null)
            {
                user.Player?.MsgLocStr("Esse jogador ainda nao tem conta bancaria.");
                return;
            }

            jogador.BankAccount.AddCurrency(moeda, quantia);
            user.Player?.MsgLocStr($"Creditado {quantia} de {moeda.Name} na conta de {jogador.Name}.");
        }

        [ChatSubCommand("Moeda", "Lista as moedas do servidor e o saldo de um jogador.", ChatAuthorizationLevel.Admin)]
        public static void Moedas(User user, User jogador = null)
        {
            var alvo = jogador ?? user;
            foreach (var c in CurrencyManager.Currencies.Where(c => c != null))
            {
                var saldo = alvo?.BankAccount != null ? alvo.BankAccount.GetCurrencyHoldingVal(c) : 0f;
                user.Player?.MsgLocStr($"[{c.Id}] {c.Name} = {saldo} (conta de {alvo?.Name ?? "-"})");
            }
        }
    }
}
