using BudzetDomowy.Core.Patterns.FactoryMethod;
using BudzetDomowy.Core.Patterns.ObserverMethod;
using BudzetDomowy.Core.Patterns.CompositeMethod;

namespace BudzetDomowy.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== BUDŻET DOMOWY ===");

            ITransactionFactory factory = new StandardTransactionFactory();
            double limit = 3000;
            BudgetManager manager = new BudgetManager(limit, factory);
            manager.AddObserver(new AlertSystem());
            manager.AddObserver(new EmailNotifier());
            bool running = true;
            while (running)
            {
                Console.WriteLine("\n------------------------------------------------");
                Console.WriteLine("MENU GŁÓWNE:");
                Console.WriteLine("1. Dodaj transakcję (Factory)");
                Console.WriteLine("2. Zmień strategię prognoz (Strategy - TODO)");
                Console.WriteLine("3. Generuj Raport (Builder - TODO)");
                Console.WriteLine("4. Pokaż drzewo kategorii");
                Console.WriteLine("9. Pokaż listę transakcji (Debug)");
                Console.WriteLine("0. Wyjdź");
                Console.Write("Wybierz opcję: ");

                var choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1":
                        HandleAddTransaction(manager);
                        break;

                    case "2":
                        Console.WriteLine("nie ma");
                        break;

                    case "3":
                        Console.WriteLine("nie ma");
                        break;

                    case "4":
                        CategoryTree.Root.Print();
                        break;

                    case "9":
                        manager.ShowCurrentTransactions();
                        break;

                    case "0":
                        running = false;
                        break;
                    default:
                        Console.WriteLine("Nieznana opcja.");
                        break;
                }
            }
        }

        static void HandleAddTransaction(BudgetManager manager)
        {
            Console.WriteLine("Typ (wydatek/przychod): ");
            string type = Console.ReadLine();
            Console.WriteLine("Opis: ");
            string desc = Console.ReadLine();
            Console.WriteLine("Kwota: ");
            if (double.TryParse(Console.ReadLine(), out double amount))
            {
                try
                {
                    manager.AddTransaction(type, amount, desc);
                    Console.WriteLine("Sukces.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Błąd fabryki: " + ex.Message);
                }
            }
        }
    }
}