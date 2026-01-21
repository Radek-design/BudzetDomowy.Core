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

                double limit = GetValidDouble("Podaj miesięczny LIMIT wydatków (cel, np. 3000): ");
                double initialBalance = GetValidDouble("Podaj SALDO początkowe (ile masz teraz pieniędzy): ");

                ITransactionFactory factory = new StandardTransactionFactory();
                BudgetManager manager = new BudgetManager(limit, initialBalance, factory);

                manager.AddObserver(new AlertSystem());
                manager.AddObserver(new EmailNotifier());

                bool running = true;
                while (running)
                {
                    try
                    {
                        ShowMenu(manager.CalculateBalance(), limit);
                        var choice = Console.ReadLine();
                        Console.WriteLine();

                        switch (choice)
                        {
                            case "1": HandleAddTransaction(manager); break;
                            case "2": HandleStrategy(manager); break;
                            case "3": HandleReport(manager); break;
                            case "4":
                                manager.ShowCategoryTree();
                                PressAnyKeyToReturn();
                                break;
                            case "5":
                                manager.ShowCurrentTransactions();
                                PressAnyKeyToReturn();
                                break;
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
            Console.WriteLine($"SALDO: {balance:0.00} PLN (Cel limitu: {limit:0.00} PLN)");
            Console.WriteLine("1. Dodaj transakcję");
            Console.WriteLine("2. Prognoza");
            Console.WriteLine("3. Raport");
            Console.WriteLine("4. Kategorie");
            Console.WriteLine("5. Lista");
            Console.WriteLine("0. Wyjdź z programu");
            Console.Write("Wybór: ");
        }

        // --- 1. DODAWANIE TRANSAKCJI (z opcją powrotu) ---
        static void HandleAddTransaction(BudgetManager manager)
        {
            Console.WriteLine(">> DODAWANIE TRANSAKCJI");
            Console.WriteLine("(Wpisz '0' w dowolnym momencie wyboru typu, aby anulować)");

            // Walidacja TYPU z opcją powrotu
            string type = "";
            while (true)
            {
                Console.Write("Typ (wydatek/przychod): ");
                string input = Console.ReadLine()?.ToLower().Trim();

                // COFANIE: Jeśli użytkownik wpisze 0, wychodzimy z metody
                if (input == "0")
                {
                    Console.WriteLine("Anulowano dodawanie.");
                    return;
                }

                if (input == "wydatek" || input == "przychod" || input == "przychód")
                {
                    type = input;
                    break;
                }
                Console.WriteLine("Błąd: Wpisz 'wydatek', 'przychod' lub '0' aby wrócić.");
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

            // Kategoria
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
                Console.Write($"\nWpisz nazwę kategorii ({type}) lub '0' aby anulować: ");
                string input = Console.ReadLine();

                if (input == "0") return; // Opcja cofnięcia na etapie kategorii

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

        // --- 2. PROGNOZA ---
        static void HandleStrategy(BudgetManager manager)
        {
            Console.WriteLine(">> WYBÓR STRATEGII");
            Console.WriteLine("1. Średnia");
            Console.WriteLine("2. Ostatni miesiąc");
            Console.WriteLine("3. Regresja");
            Console.WriteLine("4. Średnia ruchoma");
            Console.WriteLine("5. Sezonowa");
            Console.WriteLine("0. Powrót do menu"); // Opcja 0

            Console.Write("Wybór: ");
            var sChoice = Console.ReadLine();

            if (sChoice == "0") return;

            try
            {
                IForecastingStrategy strategy = sChoice switch
                {
                    "1" => new AverageForecast(),
                    "2" => new LastMonthForecast(),
                    "3" => new LinearRegressionForecast(),
                    "4" => new MovingAverageForecast(3),
                    "5" => new SeasonalForecast(),
                    _ => null
                };

                if (strategy == null)
                {
                    Console.WriteLine("Nieznana strategia. Powrót do menu.");
                    return;
                }

                manager.SetForecastingStrategy(strategy);
                Console.WriteLine($"Prognoza: {manager.GetForecast()} PLN");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd obliczeń: {ex.Message}");
            }
            PressAnyKeyToReturn();
        }

        // --- 3. RAPORT (z opcją powrotu) ---
        static void HandleReport(BudgetManager manager)
        {
            Console.WriteLine(">> GENEROWANIE RAPORTU");
            Console.WriteLine("Format: 1. PDF, 2. CSV, 0. Powrót");
            Console.Write("Wybór: ");
            string choice = Console.ReadLine();

            if (choice == "0") return; // Cofnięcie

            IReportBuilder builder = null;
            if (choice == "1") builder = new PdfReportBuilder();
            else if (choice == "2") builder = new CsvReportBuilder();
            else
            {
                Console.WriteLine("Nieznany format.");
                return;
            }

            Console.WriteLine(manager.GenerateReport(builder).content);
            PressAnyKeyToReturn();
        }

        // --- Metody Pomocnicze ---
        static void PressAnyKeyToReturn()
        {
            Console.WriteLine("\n[Naciśnij dowolny klawisz, aby wrócić do menu...]");
            Console.ReadKey();
        }

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