using BudzetDomowy.Core.Models;
using BudzetDomowy.Core.Patterns.FactoryMethod;
using BudzetDomowy.Core.Patterns.ObserverMethod;
using BudzetDomowy.Core.Patterns.StrategyMethod;
using BudzetDomowy.Core.Patterns.BuilderMethod;
using BudzetDomowy.Core.Patterns.CompositeMethod; // WAŻNE: Dodaj ten using
using System;
using System.Collections.Generic;
using System.Linq;

namespace BudzetDomowy
{
    // Główna klasa zarządcza (Context/Subject).
    // Integruje wszystkie wzorce projektowe w jedną logikę biznesową.
    public class BudgetManager
    {
        private double MonthlyLimit;
        // Lista przechowuje obiekty typu Transaction z poprawnej przestrzeni nazw
        private List<Transaction> transactions = new List<Transaction>();
        // Observer: Lista subskrybentów
        private List<IBudgetObserver> observers = new List<IBudgetObserver>();
        
        private ITransactionFactory _transactionFactory;
        private IForecastingStrategy _forecastingStrategy;

        public BudgetManager(double limit, ITransactionFactory factory)
        {
            MonthlyLimit = limit;
            _transactionFactory = factory;
            _forecastingStrategy = new AverageForecast();
        }

        // Dodaje nową transakcję, aktualizuje drzewo kategorii i powiadamia obserwatorów.
        public void AddTransaction(string type, double amount, string desc, DateTime date, string categoryName)
        {
            // 1. Factory Method: Delegacja tworzenia obiektu
            Transaction t = _transactionFactory.CreateTransaction(type, amount, desc, date, categoryName);
            transactions.Add(t);

            // 2. Composite: Aktualizacja struktury drzewiastej (tylko dla wydatków/kategorii)
            var categoryNode = CategoryTree.FindByName(categoryName);

            if (categoryNode is SingleCategory singleCat)
            {
                // Jeśli znaleziono konkretną kategorię (liść), dodajemy do niej kwotę
                // Uwaga: W drzewie zazwyczaj sumujemy tylko wydatki, ale to zależy od logiki. 
                // Tutaj dodajemy kwotę bez względu na typ, ale w raporcie drzewa widać strukturę.
                singleCat.AddAmount(amount);
                Console.WriteLine($"[Composite] Zaktualizowano kategorię '{categoryName}'.");
            }
            else
            {
                // Jeśli nie znaleziono, można np. dodać do "Inne" automatycznie,
                // albo po prostu zignorować w drzewie. Tutaj dodajemy do "Inne" jako fallback.
                var otherCat = CategoryTree.FindByName("Inne") as SingleCategory;
                otherCat?.AddAmount(amount);
                Console.WriteLine($"[Composite] Kategoria '{categoryName}' nieznana. Przypisano do 'Inne'.");
            }

            Console.WriteLine($"[Manager] Dodano: {t.GetType().Name} | {t.Description}");

            // 3. Observer notify
            if (t is Expense)
            {
                Notify();
            }
        }

        public void ShowCategoryTree()
        {
            Console.WriteLine("\n--- STRUKTURA KATEGORII (COMPOSITE) ---");
            // Wywołanie metody Print() z korzenia drzewa
            CategoryTree.Root.Print();
        }

        // Reszta metod bez zmian (Observer, Strategy, Builder)...
        public void AddObserver(IBudgetObserver observer)
        {
            if (!observers.Contains(observer)) observers.Add(observer);
        }

        public void Notify()
        {
            double balance = CalculateBalance();
            foreach (var obs in observers) obs.Update(balance, MonthlyLimit);
        }

        public void SetForecastingStrategy(IForecastingStrategy strategy)
        {
            _forecastingStrategy = strategy;
        }

        public decimal GetForecast()
        {
            return _forecastingStrategy.PredictNextMonth(transactions);
        }

        public Report GenerateReport(IReportBuilder builder)
        {
            string stats = $"Liczba transakcji: {transactions.Count}, Saldo: {CalculateBalance():F2} PLN";
            ReportDirector director = new ReportDirector(transactions, stats, "Wygenerowano przez BudżetDomowy");
            director.Construct(builder);
            return builder.GetReport();
        }

        public double CalculateBalance()
        {
            double income = transactions.OfType<Income>().Sum(t => t.Amount);
            double expense = transactions.OfType<Expense>().Sum(t => t.Amount);
            return MonthlyLimit + income - expense;
        }

        public void ShowCurrentTransactions()
        {
            Console.WriteLine("\n--- Obecne Transakcje ---");
            foreach (var t in transactions.OrderBy(x => x.Date))
                Console.WriteLine(t.ToString());
        }
    }
}