using System;

namespace BudzetDomowy.Core.Patterns.ObserverMethod
{
    // System alertów wizualnych w konsoli.
    // Reaguje na zmianę stanu budżetu, zmieniając kolor tekstu (Zielony/Czerwony).
    public class AlertSystem : IBudgetObserver
    {
        public void Update(double balance, double limit, double expenses)
        {
            bool limitExceeded = expenses > limit;
            bool balanceCritical = balance < 0;

            // 1. Sprawdzenie Limitu (Ostrzeżenie - Żółty)
            if (limitExceeded)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"[ALERT] UWAGA! Przekroczono planowany limit o {expenses - limit:0.00} PLN!");
            }

            // 2. Sprawdzenie Salda (Krytyczne - Czerwony)
            if (balanceCritical)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ALERT] ALARM KRYTYCZNY! Brak środków na koncie! Saldo: {balance:0.00} PLN");
            }

            // 3. Stan OK (Zielony) - tylko gdy żaden z powyższych nie występuje
            if (!limitExceeded && !balanceCritical)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[ALERT] Stan stabilny. Saldo: {balance:0.00} PLN. Wykorzystanie limitu: {expenses:0.00}/{limit:0.00}");
            }

            Console.ResetColor();
        }
    }
}