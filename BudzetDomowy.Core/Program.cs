using BudzetDomowy.Core.Models;
using BudzetDomowy.Core.Patterns.FactoryMethod;
using BudzetDomowy.Core.Patterns.ObserverMethod;
using BudzetDomowy.Core.Patterns.StrategyMethod;
using BudzetDomowy.Core.Patterns.BuilderMethod;
using System;

namespace BudzetDomowy.Core
{
    class Program
    {
        static void Main(string[] args)
        {
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

            Console.WriteLine("\nSystem gotowy.");

            bool running = true;
            while (running)
            {
                Console.WriteLine("\n------------------------------------------------");
                Console.WriteLine($"SALDO: {manager.CalculateBalance():0.00} PLN");
                Console.WriteLine("MENU:");
                Console.WriteLine("1. Dodaj transakcję (z wyborem daty)");
                Console.WriteLine("2. Prognozuj przyszły miesiąc");
                Console.WriteLine("3. Generuj Raport");
                Console.WriteLine("9. Pokaż listę transakcji");
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
            DateTime date;

            if (string.IsNullOrWhiteSpace(dateInput))
            {
                date = DateTime.Now;
            }
            else if (!DateTime.TryParse(dateInput, out date))
            {
                Console.WriteLine("Błąd: Niepoprawny format daty. Anulowano.");
                return;
            }

            try
            {
                manager.AddTransaction(type, amount, desc, date);
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
            Console.WriteLine("Wybierz strategię:");
            Console.WriteLine("1. Średnia (Average)");
            Console.WriteLine("2. Ostatni miesiąc (LastMonth)");
            Console.WriteLine("3. Regresja liniowa (LinearRegression)");

            var sChoice = Console.ReadLine();
            if (sChoice == "1") manager.SetForecastingStrategy(new AverageForecast());
            else if (sChoice == "2") manager.SetForecastingStrategy(new LastMonthForecast());
            else if (sChoice == "3") manager.SetForecastingStrategy(new LinearRegressionForecast());
            else
            {
                Console.WriteLine("Domyślnie: Średnia");
                manager.SetForecastingStrategy(new AverageForecast());
            }

            try
            {
                decimal forecast = manager.GetForecast();
                Console.WriteLine($"\n[PROGNOZA] Przewidywane na następny miesiąc: {forecast} PLN");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd prognozy: {ex.Message}");
                Console.WriteLine("(Upewnij się, że dodałeś transakcje z odpowiednimi datami historycznymi)");
            }
        }

        static void HandleReport(BudgetManager manager)
        {
            Console.WriteLine("Format raportu: 1. PDF, 2. CSV");
            var rChoice = Console.ReadLine();
            IReportBuilder builder = (rChoice == "2") ? new CsvReportBuilder() : new PdfReportBuilder();

            var report = manager.GenerateReport(builder);
            Console.WriteLine("\n--- RAPORT ---");
            Console.WriteLine(report.content);
        }
    }
}