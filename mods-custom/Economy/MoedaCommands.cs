using System.Linq;
using Eco.Gameplay.Economy;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;

namespace Eco.Mods.BbcBrasil
{
    // Eco has no admin command that credits an account: the Money group only manages
    // accounts, and /gabe pays every player at once. This does the one thing that was
    // missing — put an amount into a single account — through the same call the game
    // uses internally, BankAccount.AddCurrency.
    [ChatCommandHandler]
    public static class MoedaCommands
    {
        [ChatCommand("Ferramentas de moeda do servidor.", ChatAuthorizationLevel.Admin)]
        public static void Moeda() { }

        [ChatSubCommand("Moeda", "Credita moeda na conta de um jogador. Ex: /moeda dar Raposo 10000 Real", ChatAuthorizationLevel.Admin)]
        public static void Dar(User user, string jogador, float quantia, string moeda = "Real")
        {
            var currency = CurrencyManager.Currencies.FirstOrDefault(c => c != null && c.Name == moeda);
            if (currency == null)
            {
                user.Player?.MsgLocStr($"Moeda '{moeda}' nao encontrada. Use /moeda moedas para listar.");
                return;
            }

            var alvo = UserManager.Users.FirstOrDefault(u => u != null && u.Name == jogador);
            if (alvo == null)
            {
                user.Player?.MsgLocStr($"Jogador '{jogador}' nao encontrado.");
                return;
            }

            if (alvo.BankAccount == null)
            {
                user.Player?.MsgLocStr($"{alvo.Name} ainda nao tem conta bancaria.");
                return;
            }

            alvo.BankAccount.AddCurrency(currency, quantia);
            user.Player?.MsgLocStr($"Creditado {quantia} {currency.Name} na conta de {alvo.Name}.");
        }

        [ChatSubCommand("Moeda", "Lista as moedas existentes e quanto ha na conta de um jogador.", ChatAuthorizationLevel.Admin)]
        public static void Moedas(User user, string jogador = "")
        {
            var alvo = string.IsNullOrWhiteSpace(jogador)
                ? user
                : UserManager.Users.FirstOrDefault(u => u != null && u.Name == jogador);

            foreach (var c in CurrencyManager.Currencies.Where(c => c != null))
            {
                var saldo = alvo?.BankAccount != null ? alvo.BankAccount.GetCurrencyHoldingVal(c) : 0f;
                user.Player?.MsgLocStr($"{c.Name}: {saldo} (conta de {alvo?.Name ?? "-"})");
            }
        }
    }
}
