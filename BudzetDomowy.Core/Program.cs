using BudzetDomowy.Core.Models;
using BudzetDomowy.Core.Patterns.FactoryMethod;
using BudzetDomowy.Core.Patterns.ObserverMethod;
using BudzetDomowy.Core.Patterns.StrategyMethod;
using BudzetDomowy.Core.Patterns.BuilderMethod;
using BudzetDomowy.Core.Patterns.CompositeMethod;
using System;
using System.Collections.Generic;
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

                // 1. Ustawienie Limitu (Budżetu) - Cel
                double limit = GetValidDouble("Podaj miesięczny LIMIT wydatków (cel, np. 3000): ");

                // 2. Ustawienie Salda Początkowego (Stan konta) - Rzeczywistość
                double initialBalance = GetValidDouble("Podaj SALDO początkowe (ile masz teraz pieniędzy): ");

                ITransactionFactory factory = new StandardTransactionFactory();

                // Przekazujemy obie wartości do managera
                BudgetManager manager = new BudgetManager(limit, initialBalance, factory);

                manager.AddObserver(new AlertSystem());
                manager.AddObserver(new EmailNotifier());

                bool running = true;
                while (running)
                {
                    try
                    {
                        // Wyświetlamy saldo (Limit jest w nawiasie jako info)
                        ShowMenu(manager.CalculateBalance(), limit);
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

        static void ShowMenu(double balance, double limit)
        {
            Console.WriteLine("\n------------------------------------------------");
            // Wyświetlamy rzeczywiste saldo
            Console.WriteLine($"SALDO: {balance:0.00} PLN (Twój cel limitu: {limit:0.00} PLN)");
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
            Console.WriteLine(">> DODAWANIE TRANSAKCJI");

            // Walidacja TYPU
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

            Console.Write("Opis: ");
            string desc = Console.ReadLine();

            double amount = GetValidDouble("Kwota (musi być > 0): ");
            while (amount <= 0)
            {
                Console.WriteLine("Kwota musi być dodatnia.");
                amount = GetValidDouble("Kwota: ");
            }

            DateTime date = GetValidDate();

            // Walidacja KATEGORII (Przychod -> Gałąź PRZYCHODY, Wydatek -> Gałąź WYDATKI)
            string rootBranchName = (type == "wydatek") ? "WYDATKI" : "PRZYCHODY";
            Console.WriteLine($"\n--- Wybierz kategorię z grupy {rootBranchName} ---");

            var branchGroup = CategoryTree.GetGroupByName(rootBranchName);
            List<string> validCategories = new List<string>();

            if (branchGroup != null)
            {
                CollectCategoryNames(branchGroup, validCategories);
                foreach (var catName in validCategories) Console.WriteLine($"- {catName}");
            }

            string category = "";
            while (true)
            {
                Console.Write($"\nWpisz nazwę kategorii ({type}): ");
                string input = Console.ReadLine();
                if (validCategories.Contains(input, StringComparer.OrdinalIgnoreCase))
                {
                    category = input;
                    break;
                }
                Console.WriteLine($"Błąd: Kategoria '{input}' nie pasuje do typu {type}.");
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
                if (string.IsNullOrWhiteSpace(input)) return DateTime.Now;
                if (DateTime.TryParse(input, out DateTime date)) return date;
                Console.WriteLine("Błąd: Niepoprawny format daty.");
            }
        }

        static void CollectCategoryNames(CategoryComponent component, List<string> list)
        {
            if (component is SingleCategory) list.Add(component.Name);
            else if (component is CategoryGroup group)
            {
                foreach (var child in group.GetChildren()) CollectCategoryNames(child, list);
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