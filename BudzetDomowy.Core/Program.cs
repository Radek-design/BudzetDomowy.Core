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
            // Ustawienie licencji dla biblioteki QuestPDF (wymagane w wersji Community)
            QuestPDF.Settings.License = LicenseType.Community;

            Console.WriteLine("=== SYSTEM BUDŻET DOMOWY ===");

            Console.Write("Podaj miesięczny limit wydatków (PLN): ");
            double limit;
            while (!double.TryParse(Console.ReadLine(), out limit))
            {
                Console.Write("Błędna wartość. Podaj liczbę: ");
            }

            // Inicjalizacja Fabryki i Managera (Dependency Injection)
            ITransactionFactory factory = new StandardTransactionFactory();
            BudgetManager manager = new BudgetManager(limit, factory);

            // Rejestracja Obserwatorów
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
                        // Wyświetlanie struktury drzewiastej (Composite)
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

        // Obsługuje interakcję z użytkownikiem przy dodawaniu nowej transakcji.
        // Pobiera dane (typ, kwota, data, kategoria) i zleca dodanie do Managera.
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

            // Obsługa daty (przywrócona funkcjonalność)
            Console.Write("Data (RRRR-MM-DD) [Enter = dzisiaj]: ");
            string dateInput = Console.ReadLine();
            DateTime date = string.IsNullOrWhiteSpace(dateInput)
                ? DateTime.Now
                : (DateTime.TryParse(dateInput, out var d) ? d : DateTime.Now);

            // Obsługa kategorii (Composite)
            Console.WriteLine("\nDostępne kategorie:");
            CategoryTree.Root.Print(); // Wyświetlamy drzewo pomocniczo

            Console.Write("\nWpisz nazwę kategorii z listy powyżej (np. 'Sklep', 'Czynsz'): ");
            string category = Console.ReadLine();

            try
            {
                // Przekazujemy wszystkie dane (w tym datę i kategorię) do managera
                manager.AddTransaction(type, amount, desc, date, category);
                Console.WriteLine("Sukces.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("BŁĄD: " + ex.Message);
            }
        }

        // Obsługuje wybór strategii prognozowania i wyświetla wynik.
        static void HandleStrategy(BudgetManager manager)
        {
            Console.WriteLine(">> PROGNOZOWANIE");
            Console.WriteLine("1. Średnia (Average)");
            Console.WriteLine("2. Ostatni miesiąc (LastMonth)");
            Console.WriteLine("3. Regresja liniowa (LinearRegression)");
            Console.WriteLine("4. Średnia ruchoma (MovingAverage - 3 msc)");
            Console.WriteLine("5. Sezonowa (Seasonal)");
            Console.Write("Twój Wybor: ");
            var sChoice = Console.ReadLine();

            // Dobór strategii w czasie rzeczywistym (Runtime)
            IForecastingStrategy strategy = sChoice switch
            {
                "2" => new LastMonthForecast(),
                "3" => new LinearRegressionForecast(),
                "4" => new MovingAverageForecast(3),
                "5" => new SeasonalForecast(),
                _ => new AverageForecast() // Domyślnie
            };

            manager.SetForecastingStrategy(strategy);

            try
            {
                Console.WriteLine($"Prognoza: {manager.GetForecast()} PLN");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd: {ex.Message}");
            }
        }

        // Obsługuje generowanie raportów przy użyciu wzorca Builder.
        static void HandleReport(BudgetManager manager)
        {
            Console.WriteLine(">> GENEROWANIE RAPORTU DO PLIKU");
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
            Console.WriteLine(report.content); // Wyświetla ścieżkę do pliku
            Console.WriteLine("------------------------------------------------\n");
        }
    }
}