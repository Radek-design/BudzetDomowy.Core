using System;

namespace BudzetDomowy.Core.Patterns.ObserverMethod
{
    // Symulator systemu powiadomień e-mail.
    // Wysyła powiadomienie tylko w sytuacji krytycznej (ujemne saldo).
    public class EmailNotifier : IBudgetObserver
    {
        public void Update(double balance, double limit)
        {
            // Logika biznesowa: Nie spamujemy użytkownika, jeśli wszystko jest w porządku.
            if (balance < 0)
            {
                Console.WriteLine($"[EMAIL] Wysłano powiadomienie: przekroczono limit {limit:0.00} PLN. Saldo: {balance:0.00} PLN.");
            }
        }
    }
}