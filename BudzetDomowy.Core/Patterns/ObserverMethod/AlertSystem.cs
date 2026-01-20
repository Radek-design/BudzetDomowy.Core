using System;

namespace BudzetDomowy.Core.Patterns.ObserverMethod
{
    public class AlertSystem : IBudgetObserver
    {
        public void Update(double balance, double limit)
        {
            // zielony gdy jest ok, czerwony gdy przekroczony limit (saldo < 0)
            if (balance >= 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[ALERT] OK. Saldo: {balance:0.00} PLN (limit: {limit:0.00} PLN)");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ALERT] PRZEKROCZONO LIMIT! Saldo: {balance:0.00} PLN (limit: {limit:0.00} PLN)");
            }

            Console.ResetColor();
        }
    }
}