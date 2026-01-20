using BudzetDomowy.Core.Models;
using BudzetDomowy.Core.Patterns.FactoryMethod;
using BudzetDomowy.Core.Patterns.ObserverMethod;
using BudzetDomowy.Core.Patterns.StrategyMethod;
using BudzetDomowy.Core.Patterns.BuilderMethod;
using BudzetDomowy.Core.Patterns.CompositeMethod; // Using do Composite
using System;

namespace BudzetDomowy.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            // QuestPDF Settings (jeśli używasz generowania PDF)
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            Console.WriteLine("=== SYSTEM BUDŻET DOMOWY ===");

            Console.Write("Podaj miesięczny limit wydatków (PLN): ");
            double limit;
            while (!double.TryParse(Console.ReadLine(), out limit))
            {
                Console.Write("Błędna wartość. Podaj liczbę: ");
            }

            ITransactionFactory factory = new StandardTransactionFactory();
            BudgetManager manager = new BudgetManager(limit, factory);

            manager.AddObserver(new AlertSystem());
            manager.AddObserver(new EmailNotifier());

            bool running = true;
            while (running)
            {
                Console.WriteLine("\n------------------------------------------------");
                Console.WriteLine($"SALDO: {manager.CalculateBalance():0.00} PLN");
                Console.WriteLine("MENU:");
                Console.WriteLine("1. Dodaj transakcję (Factory + Observer + Composite)");
                Console.WriteLine("2. Prognozuj przyszły miesiąc (Strategy)");
                Console.WriteLine("3. Generuj Raport (Builder)");
                Console.WriteLine("4. Pokaż drzewo kategorii (Composite)");
                Console.WriteLine("5. Pokaż listę transakcji");
                Console.WriteLine("0. Wyjdź");
                Console.Write("Twój wybór: ");

                var choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1":
                        HandleAddTransaction(manager);
                        break;
                    case "2":
                        HandleStrategy(manager);
                        break;
                    case "3":
                        HandleReport(manager);
                        break;
                    case "4":
                        // NOWOŚĆ: Wyświetlanie drzewa
                        manager.ShowCategoryTree();
                        break;
                    case "5":
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
            Console.WriteLine(">> DODAWANIE TRANSAKCJI");

            Console.Write("Typ (wydatek/przychod): ");
            string type = Console.ReadLine();

            Console.Write("Opis: ");
            string desc = Console.ReadLine();

            Console.Write("Kwota: ");
            if (!double.TryParse(Console.ReadLine(), out double amount))
            {
                Console.WriteLine("Błąd: Zła kwota.");
                return;
            }

            Console.Write("Data (RRRR-MM-DD) [Enter = dzisiaj]: ");
            string dateInput = Console.ReadLine();
            DateTime date = string.IsNullOrWhiteSpace(dateInput)
                ? DateTime.Now
                : (DateTime.TryParse(dateInput, out var d) ? d : DateTime.Now);

            // OBSŁUGA COMPOSITE: Wybór kategorii
            Console.WriteLine("\nDostępne kategorie:");
            // Wyświetlamy drzewo, żeby użytkownik wiedział co wpisać
            CategoryTree.Root.Print();

            Console.Write("\nWpisz nazwę kategorii z listy powyżej (np. 'Sklep', 'Czynsz'): ");
            string category = Console.ReadLine();

            try
            {
                // Przekazujemy kategorię do managera
                manager.AddTransaction(type, amount, desc, date, category);
                Console.WriteLine("Sukces.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("BŁĄD: " + ex.Message);
            }
        }

        static void HandleStrategy(BudgetManager manager)
        {
            Console.WriteLine(">> PROGNOZOWANIE");
            Console.WriteLine("1. Średnia");
            Console.WriteLine("2. Ostatni miesiąc");
            Console.WriteLine("3. Regresja liniowa");

            var sChoice = Console.ReadLine();
            if (sChoice == "1") manager.SetForecastingStrategy(new AverageForecast());
            else if (sChoice == "2") manager.SetForecastingStrategy(new LastMonthForecast());
            else if (sChoice == "3") manager.SetForecastingStrategy(new LinearRegressionForecast());
            else manager.SetForecastingStrategy(new AverageForecast());

            try
            {
                Console.WriteLine($"Prognoza: {manager.GetForecast()} PLN");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd: {ex.Message}");
            }
        }

        static void HandleReport(BudgetManager manager)
        {
            Console.WriteLine(">> GENEROWANIE RAPORTU DO PLIKU (BUILDER)");
            Console.WriteLine("Wybierz format:");
            Console.WriteLine("1. PDF");
            Console.WriteLine("2. CSV");

            var choice = Console.ReadLine();
            IReportBuilder builder = null;

            if (choice == "1")
                builder = new PdfReportBuilder();
            else if (choice == "2")
                builder = new CsvReportBuilder();
            else
            {
                Console.WriteLine("Błąd wyboru.");
                return;
            }

            var report = manager.GenerateReport(builder);
            Console.WriteLine("\n------------------------------------------------");
            Console.WriteLine(report.content);
            Console.WriteLine("------------------------------------------------\n");
        }
    }
}