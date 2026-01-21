using System;

namespace BudzetDomowy.Core.Patterns.ObserverMethod
{
    // Symulator systemu powiadomień e-mail.
    // Wysyła powiadomienie tylko w sytuacji krytycznej (ujemne saldo).
    public class EmailNotifier : IBudgetObserver
    {
        public void Update(double balance, double limit, double expenses)
        {
            // Powiadomienie o przekroczeniu budżetu
            if (expenses > limit)
            {
                Console.WriteLine($"[EMAIL] Powiadomienie: Wydałeś już {expenses:0.00} PLN, co przekracza ustalony limit {limit:0.00} PLN.");
            }

            // Osobne, pilniejsze powiadomienie o ujemnym saldzie
            if (balance < 0)
            {
                Console.WriteLine($"[EMAIL] PILNE: Twoje saldo wynosi {balance:0.00} PLN. Wpłać środki!");
            }
        }
    }
}