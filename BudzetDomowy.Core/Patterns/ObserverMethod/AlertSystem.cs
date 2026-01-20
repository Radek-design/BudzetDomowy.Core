using System;

namespace BudzetDomowy.Core.Patterns.ObserverMethod
{
    // System alertów wizualnych w konsoli.
    // Reaguje na zmianę stanu budżetu, zmieniając kolor tekstu (Zielony/Czerwony).
    public class AlertSystem : IBudgetObserver
    {
        public void Update(double balance, double limit)
        {
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