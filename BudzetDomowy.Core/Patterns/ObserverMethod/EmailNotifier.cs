using System;

namespace BudzetDomowy.Core.Patterns.ObserverMethod
{
    public class EmailNotifier : IBudgetObserver
    {
        public void Update(double balance, double limit)
        {
            if (balance < 0)
            {
                Console.WriteLine($"[EMAIL] Wysłano powiadomienie: przekroczono limit {limit:0.00} PLN. Saldo: {balance:0.00} PLN.");
            }
        }
    }
}