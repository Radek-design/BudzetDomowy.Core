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
                // Ustawienie licencji QuestPDF
                QuestPDF.Settings.License = LicenseType.Community;
                Console.WriteLine("=== SYSTEM BUDŻET DOMOWY ===");

                ITransactionFactory factory = new StandardTransactionFactory();
                BudgetManager manager = new BudgetManager(0, 0, factory);

                // Próba odczytu danych przy starcie
                if (manager.LoadData())
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(">> Wczytano dane.");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine(">> Konfiguracja nowej sesji.");
                    double limit = GetValidDouble("Podaj miesięczny LIMIT wydatków: ");
                    double initial = GetValidDouble("Podaj SALDO początkowe: ");
                    manager = new BudgetManager(limit, initial, factory);
                }

                // Dodanie obserwatorów
                manager.AddObserver(new AlertSystem());
                manager.AddObserver(new EmailNotifier());

                bool running = true;
                while (running)
                {
                    try
                    {
                        ShowMenu(manager.CalculateBalance(), manager.MonthlyLimit);
                        string choice = Console.ReadLine();
                        Console.WriteLine();

                        switch (choice)
                        {
                            case "1": HandleAddTransaction(manager); break;
                            case "2": HandleStrategy(manager); break;
                            case "3": HandleReport(manager); break;
                            case "4": manager.ShowCategoryTree(); PressAnyKeyToReturn(); break;
                            case "5": manager.ShowCurrentTransactions(); PressAnyKeyToReturn(); break;
                            case "6": HandleManageCategories(manager); break; // ZMIANA
                            case "0": manager.SaveData(); running = false; break;
                            default: Console.WriteLine("Nieznana opcja."); break;
                        }
                    }
                    catch (Exception ex) { Console.WriteLine($"[BŁĄD]: {ex.Message}"); }
                }
            }
            catch (Exception ex) { Console.WriteLine($"[CRITICAL]: {ex.Message}"); }
        }
        // --- MENU GŁÓWNE ---
        static void ShowMenu(double balance, double limit)
        {
            Console.WriteLine("\n------------------------------------------------");
            Console.WriteLine($"SALDO: {balance:0.00} PLN (Cel: {limit:0.00})");
            Console.WriteLine("1. Dodaj transakcję");
            Console.WriteLine("2. Prognoza");
            Console.WriteLine("3. Raport");
            Console.WriteLine("4. Pokaż Kategorie");
            Console.WriteLine("5. Pokaż Transakcje");
            Console.WriteLine("6. ZARZĄDZAJ KATEGORIAMI (+/-)");
            Console.WriteLine("0. Zapisz i Wyjdź");
            Console.Write("Wybór: ");
        }

        // --- PODMENU ZARZĄDZANIA KATEGORIAMI ---
        static void HandleManageCategories(BudgetManager manager)
        {
            Console.WriteLine("\n>> ZARZĄDZANIE KATEGORIAMI");
            Console.WriteLine("1. Dodaj Podkategorię (Liść, np. 'Paliwo')");
            Console.WriteLine("2. Dodaj Grupę Nadrzędną (Węzeł, np. 'Inwestycje')");
            Console.WriteLine("3. Usuń Kategorię/Grupę");
            Console.WriteLine("0. Powrót");
            Console.Write("Wybór: ");

            var choice = Console.ReadLine();
            if (choice == "0") return;

            Console.WriteLine("\n--- AKTUALNE DRZEWO ---");
            CategoryTree.Root.Print();
            Console.WriteLine("-----------------------");

            try
            {
                if (choice == "1" || choice == "2")
                {
                    bool isGroup = (choice == "2");
                    string typeName = isGroup ? "GRUPĘ" : "PODKATEGORIĘ";

                    Console.Write($"\nWpisz nazwę RODZICA (gdzie dodać {typeName}): ");
                    string parent = Console.ReadLine();

                    Console.Write($"Wpisz nazwę NOWEJ {typeName}: ");
                    string name = Console.ReadLine();

                    if (!string.IsNullOrWhiteSpace(parent) && !string.IsNullOrWhiteSpace(name))
                    {
                        manager.AddNewCategory(parent, name, isGroup);
                        Console.WriteLine(">> Sukces.");
                    }
                }
                else if (choice == "3")
                {
                    Console.Write("\nWpisz nazwę kategorii do USUNIĘCIA: ");
                    string name = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        manager.RemoveCategory(name);
                        Console.WriteLine(">> Sukces. Usunięto.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BŁĄD: {ex.Message}");
            }
            PressAnyKeyToReturn();
        }

        // / --- DODAWANIE TRANSAKCJI ---
        static void HandleAddTransaction(BudgetManager manager)
        {
            Console.WriteLine(">> DODAWANIE TRANSAKCJI (0=Anuluj)");
            // Typ
            string type = "";
            while (true)
            {
                Console.Write("Typ (wydatek/przychod): ");
                string t = Console.ReadLine()?.ToLower().Trim();
                if (t == "0") return;
                if (t == "wydatek" || t == "przychod" || t == "przychód") { type = t; break; }
            }
            // Opis
            Console.Write("Opis: "); string desc = Console.ReadLine();
            // Kwota
            double amount = GetValidDouble("Kwota: ");
            if (amount <= 0) { Console.WriteLine("Musi być >0"); amount = GetValidDouble("Kwota: "); }
            // Data
            DateTime date = GetValidDate();

            // Kategoria
            string root = (type == "wydatek") ? "WYDATKI" : "PRZYCHODY";
            Console.WriteLine($"\nDostępne w grupie {root}:");
            var group = CategoryTree.GetGroupByName(root);
            List<string> valids = new List<string>();
            if (group != null) CollectCategoryNames(group, valids);
            foreach (var v in valids) Console.WriteLine("- " + v);

            string cat = "";
            while (true)
            {
                Console.Write("Kategoria: ");
                string c = Console.ReadLine();
                if (c == "0") return;
                if (valids.Contains(c, StringComparer.OrdinalIgnoreCase)) { cat = c; break; }
                Console.WriteLine("Błędna kategoria (musi być liściem z listy powyżej).");
            }

            manager.AddTransaction(type, amount, desc, date, cat);
        }

       // --- PODMENU STRATEGII PROGNOZOWANIA ---
        static void HandleStrategy(BudgetManager m)
        {
            Console.WriteLine("1.Średnia 2.Ostatni 3.Regresja 4.Ruchoma 5.Sezonowa 0.Wróć");
            var c = Console.ReadLine(); if (c == "0") return;
            try
            {
                IForecastingStrategy s = c switch { "2" => new LastMonthForecast(), "3" => new LinearRegressionForecast(), "4" => new MovingAverageForecast(3), "5" => new SeasonalForecast(), _ => new AverageForecast() };
                m.SetForecastingStrategy(s); Console.WriteLine($"Prognoza: {m.GetForecast()}");
            }
            catch (Exception e) { Console.WriteLine(e.Message); }
            PressAnyKeyToReturn();
        }
        // --- PODMENU RAPORTÓW ---
        static void HandleReport(BudgetManager m)
        {
            Console.WriteLine("1.PDF 2.CSV 0.Wróć");
            var c = Console.ReadLine(); if (c == "0") return;
            IReportBuilder b = c == "2" ? new CsvReportBuilder() : new PdfReportBuilder();
            Console.WriteLine(m.GenerateReport(b).content);
            PressAnyKeyToReturn();
        }
        static void PressAnyKeyToReturn() { Console.WriteLine("\n[Enter]..."); Console.ReadLine(); }
        static double GetValidDouble(string p)
        {
            while (true) { Console.Write(p); if (double.TryParse(Console.ReadLine(), out double v)) return v; }
        }
        // -- POBIERANIE POPRAWNEJ DATY ---
        static DateTime GetValidDate()
        {
            while (true)
            {
                Console.Write("Data (RRRR-MM-DD): "); string s = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(s)) return DateTime.Now;
                if (DateTime.TryParse(s, out DateTime d)) return d;
            }
        }
        // --- ZBIERANIE NAZW KATEGORII REKURENCYJNIE ---
        static void CollectCategoryNames(CategoryComponent c, List<string> l)
        {
            if (c is SingleCategory) l.Add(c.Name);
            else if (c is CategoryGroup g) foreach (var child in g.GetChildren()) CollectCategoryNames(child, l);
        }
    }
}