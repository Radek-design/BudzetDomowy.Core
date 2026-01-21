using BudzetDomowy.Core.Models;
using BudzetDomowy.Core.Patterns.FactoryMethod;
using BudzetDomowy.Core.Patterns.ObserverMethod;
using BudzetDomowy.Core.Patterns.StrategyMethod;
using BudzetDomowy.Core.Patterns.BuilderMethod;
using BudzetDomowy.Core.Patterns.CompositeMethod;
using System;
using System.Collections.Generic; // Potrzebne do List<>
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
                            default: Console.WriteLine(">> Nieznana opcja."); break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[BŁĄD MENU]: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AWARIA SYSTEMU]: {ex.Message}");
            }
        }

        static void HandleAddTransaction(BudgetManager manager)
        {
            Console.WriteLine(">> DODAWANIE TRANSAKCJI");

            // 1. Walidacja TYPU
            string type = "";
            while (true)
            {
                Console.Write("Typ (wydatek/przychod): ");
                string input = Console.ReadLine()?.ToLower().Trim();
                if (input == "wydatek" || input == "przychod" || input == "przychód")
                {
                    type = input;
                    break;
                }
                Console.WriteLine("Błąd: Wpisz 'wydatek' lub 'przychod'.");
            }

            // 2. Pobieranie OPISU i KWOTY
            Console.Write("Opis: ");
            string desc = Console.ReadLine();
            double amount = GetValidDouble("Kwota (musi być > 0): ");
            while (amount <= 0)
            {
                Console.WriteLine("Kwota musi być dodatnia.");
                amount = GetValidDouble("Kwota: ");
            }

            // 3. Walidacja DATY (Nowość)
            DateTime date = GetValidDate();

            // 4. Walidacja KATEGORII (Nowość - ścisłe powiązanie z typem)
            string rootBranchName = (type == "wydatek") ? "WYDATKI" : "PRZYCHODY";

            Console.WriteLine($"\n--- Wybierz kategorię z grupy {rootBranchName} ---");

            // Pobieramy odpowiednią gałąź drzewa
            var branchGroup = CategoryTree.GetGroupByName(rootBranchName);

            // Pobieramy listę wszystkich dostępnych nazw w tej gałęzi (helper poniżej)
            List<string> validCategories = new List<string>();
            if (branchGroup != null)
            {
                CollectCategoryNames(branchGroup, validCategories);
                // Wyświetlamy użytkownikowi listę
                foreach (var catName in validCategories)
                {
                    Console.WriteLine($"- {catName}");
                }
            }

            string category = "";
            while (true)
            {
                Console.Write($"\nWpisz nazwę kategorii ({type}): ");
                string input = Console.ReadLine();

                // Sprawdzamy czy wpisana nazwa istnieje na liście dozwolonych
                if (validCategories.Contains(input, StringComparer.OrdinalIgnoreCase))
                {
                    category = input; // Znaleziono poprawną
                    break;
                }
                Console.WriteLine($"Błąd: Kategoria '{input}' nie istnieje w grupie {rootBranchName} lub zrobiłeś literówkę.");
            }

            try
            {
                manager.AddTransaction(type, amount, desc, date, category);
                Console.WriteLine(">> Sukces: Transakcja dodana.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("BŁĄD: " + ex.Message);
            }
        }

        // --- Metody Pomocnicze ---

        static DateTime GetValidDate()
        {
            while (true)
            {
                Console.Write("Data (RRRR-MM-DD) [Enter = dzisiaj]: ");
                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    return DateTime.Now;
                }

                if (DateTime.TryParse(input, out DateTime date))
                {
                    return date;
                }

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Błąd: Niepoprawny format lub nieistniejąca data (np. 30 lutego). Spróbuj ponownie.");
                Console.ResetColor();
            }
        }

        static void CollectCategoryNames(CategoryComponent component, List<string> list)
        {
            // Jeśli to liść (SingleCategory), dodajemy do listy
            if (component is SingleCategory)
            {
                list.Add(component.Name);
            }
            // Jeśli to grupa, wchodzimy głębiej
            else if (component is CategoryGroup group)
            {
                foreach (var child in group.GetChildren())
                {
                    CollectCategoryNames(child, list);
                }
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

        static void HandleStrategy(BudgetManager manager)
        {
            Console.WriteLine("1. Średnia, 2. Ostatni miesiąc, 3. Regresja, 4. Średnia ruchoma, 5. Sezonowa");
            var sChoice = Console.ReadLine();
            // ... (reszta logiki strategii bez zmian) ...
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
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }

        static void HandleReport(BudgetManager manager)
        {
            Console.WriteLine("Format: 1. PDF, 2. CSV");
            string choice = Console.ReadLine();
            IReportBuilder builder = choice == "2" ? new CsvReportBuilder() : new PdfReportBuilder();
            Console.WriteLine(manager.GenerateReport(builder).content);
        }

        static double GetValidDouble(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (double.TryParse(Console.ReadLine(), out double result)) return result;
                Console.WriteLine("To nie jest poprawna liczba.");
            }
        }
    }
}