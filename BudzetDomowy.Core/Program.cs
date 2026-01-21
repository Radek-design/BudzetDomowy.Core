using BudzetDomowy.Core.Models;
using BudzetDomowy.Core.Patterns.FactoryMethod;
using BudzetDomowy.Core.Patterns.ObserverMethod;
using BudzetDomowy.Core.Patterns.StrategyMethod;
using BudzetDomowy.Core.Patterns.BuilderMethod;
using BudzetDomowy.Core.Patterns.CompositeMethod;
using System;
using QuestPDF.Infrastructure;

namespace BudzetDomowy.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                QuestPDF.Settings.License = LicenseType.Community;

                Console.WriteLine("=== SYSTEM BUDŻET DOMOWY ===");

                // Zabezpieczone pobieranie limitu
                double limit = GetValidDouble("Podaj miesięczny limit wydatków (PLN): ");

                ITransactionFactory factory = new StandardTransactionFactory();
                BudgetManager manager = new BudgetManager(limit, factory);

                manager.AddObserver(new AlertSystem());
                manager.AddObserver(new EmailNotifier());

                bool running = true;
                while (running)
                {
                    try
                    {
                        ShowMenu(manager.CalculateBalance());
                        var choice = Console.ReadLine();
                        Console.WriteLine();

                        switch (choice)
                        {
                            case "1": HandleAddTransaction(manager); break;
                            case "2": HandleStrategy(manager); break;
                            case "3": HandleReport(manager); break;
                            case "4": manager.ShowCategoryTree(); break;
                            case "5": manager.ShowCurrentTransactions(); break;
                            case "0": running = false; break;
                            default: Console.WriteLine(">> Nieznana opcja. Spróbuj ponownie."); break;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Wyłapuje błędy wewnątrz pętli menu, żeby program się nie zamknął
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[BŁĄD KRYTYCZNY W MENU]: {ex.Message}");
                        Console.ResetColor();
                    }
                }
            }
            catch (Exception ex)
            {
                // Błąd przy inicjalizacji (np. brak biblioteki PDF)
                Console.WriteLine($"[AWARIA SYSTEMU]: {ex.Message}");
            }
        }

        static void ShowMenu(double balance)
        {
            Console.WriteLine("\n------------------------------------------------");
            Console.WriteLine($"SALDO: {balance:0.00} PLN");
            Console.WriteLine("1. Dodaj transakcję");
            Console.WriteLine("2. Prognoza");
            Console.WriteLine("3. Raport");
            Console.WriteLine("4. Kategorie");
            Console.WriteLine("5. Lista");
            Console.WriteLine("0. Wyjdź");
            Console.Write("Wybór: ");
        }

        static void HandleAddTransaction(BudgetManager manager)
        {
            try
            {
                Console.WriteLine(">> DODAWANIE TRANSAKCJI");

                Console.Write("Typ (wydatek/przychod): ");
                string type = Console.ReadLine();
                // Prosta walidacja typu
                if (type.ToLower() != "wydatek" && type.ToLower() != "przychod" && type.ToLower() != "przychód")
                {
                    throw new ArgumentException("Niepoprawny typ. Wpisz 'wydatek' lub 'przychod'.");
                }

                Console.Write("Opis: ");
                string desc = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(desc)) desc = "Bez opisu";

                double amount = GetValidDouble("Kwota (musi być > 0): ");
                if (amount <= 0) throw new ArgumentException("Kwota musi być dodatnia.");

                Console.Write("Data (RRRR-MM-DD) [Enter = dzisiaj]: ");
                string dateInput = Console.ReadLine();
                DateTime date = string.IsNullOrWhiteSpace(dateInput)
                    ? DateTime.Now
                    : (DateTime.TryParse(dateInput, out var d) ? d : DateTime.Now);

                // Wyświetlamy kategorie, żeby użytkownik wiedział gdzie trafi jego przychód/wydatek
                Console.WriteLine("\n--- Dostępne Kategorie ---");
                CategoryTree.Root.Print();
                Console.WriteLine("--------------------------");

                Console.Write("Wpisz nazwę kategorii z listy: ");
                string category = Console.ReadLine();

                manager.AddTransaction(type, amount, desc, date, category);
                Console.WriteLine(">> Sukces: Transakcja dodana.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[BŁĄD DODAWANIA]: {ex.Message}");
                Console.ResetColor();
            }
        }

        static void HandleStrategy(BudgetManager manager)
        {
            Console.WriteLine("1. Średnia, 2. Ostatni miesiąc, 3. Regresja, 4. Średnia ruchoma, 5. Sezonowa");
            var sChoice = Console.ReadLine();

            try
            {
                IForecastingStrategy strategy = sChoice switch
                {
                    "2" => new LastMonthForecast(),
                    "3" => new LinearRegressionForecast(),
                    "4" => new MovingAverageForecast(3),
                    "5" => new SeasonalForecast(),
                    _ => new AverageForecast()
                };

                manager.SetForecastingStrategy(strategy);
                Console.WriteLine($"Prognoza: {manager.GetForecast()} PLN");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[BŁĄD PROGNOZY]: {ex.Message}");
                Console.ResetColor();
            }
        }

        static void HandleReport(BudgetManager manager)
        {
            try
            {
                Console.WriteLine("Format: 1. PDF, 2. CSV");
                string choice = Console.ReadLine();
                IReportBuilder builder = choice == "2" ? new CsvReportBuilder() : new PdfReportBuilder();

                var report = manager.GenerateReport(builder);
                Console.WriteLine(report.content);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[BŁĄD RAPORTU]: Nie udało się wygenerować pliku. {ex.Message}");
                Console.ResetColor();
            }
        }

        // Pomocnicza metoda do bezpiecznego pobierania liczb
        static double GetValidDouble(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine();
                if (double.TryParse(input, out double result))
                {
                    return result;
                }
                Console.WriteLine("To nie jest poprawna liczba. Spróbuj np. 150,50");
            }
        }
    }
}