using BudzetDomowy.Core.Models;
using BudzetDomowy.Core.Patterns.FactoryMethod;
using BudzetDomowy.Core.Patterns.ObserverMethod;
using BudzetDomowy.Core.Patterns.StrategyMethod;
using BudzetDomowy.Core.Patterns.BuilderMethod;
using BudzetDomowy.Core.Patterns.CompositeMethod;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BudzetDomowy
{
    // Główna klasa zarządcza (Context/Subject).
    // Integruje wszystkie wzorce: zleca tworzenie obiektów fabryce, powiadamia obserwatorów,
    // wykorzystuje strategię do prognozowania i zarządza strukturą kategorii.
    public class BudgetManager
    {
        private double MonthlyLimit;
        private List<Transaction> transactions = new List<Transaction>();
        private List<IBudgetObserver> observers = new List<IBudgetObserver>();

        private ITransactionFactory _transactionFactory;
        private IForecastingStrategy _forecastingStrategy;

        public BudgetManager(double limit, ITransactionFactory factory)
        {
            MonthlyLimit = limit;
            _transactionFactory = factory;
            _forecastingStrategy = new AverageForecast(); // Strategia domyślna
        }

        // Dodaje transakcję, aktualizuje drzewo kategorii i uruchamia system powiadomień.
        public void AddTransaction(string type, double amount, string desc, DateTime date, string categoryName)
        {
            // 1. Użycie Factory Method
            Transaction t = _transactionFactory.CreateTransaction(type, amount, desc, date, categoryName);
            transactions.Add(t);

            // 2. Integracja z Composite - aktualizacja wartości w drzewie
            var categoryNode = CategoryTree.FindByName(categoryName);
            if (categoryNode is SingleCategory singleCat)
            {
                singleCat.AddAmount(amount);
            }
            else
            {
                var otherCat = CategoryTree.FindByName("Inne") as SingleCategory;
                otherCat?.AddAmount(amount);
            }

            Console.WriteLine($"[Manager] Dodano: {t.GetType().Name} | {t.Description}");

            // 3. Powiadomienie Obserwatorów (tylko przy wydatku)
            if (t is Expense)
            {
                Notify();
            }
        }

        public void Notify()
        {
            double balance = CalculateBalance();
            foreach (var obs in observers) obs.Update(balance, MonthlyLimit);
        }

        public void AddObserver(IBudgetObserver observer)
        {
            if (!observers.Contains(observer)) observers.Add(observer);
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
            string stats = $"Transakcje: {transactions.Count}, Saldo: {CalculateBalance():F2} PLN";
            ReportDirector director = new ReportDirector(transactions, stats, "Raport Budżetowy");
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
            Console.WriteLine("\n--- Transakcje ---");
            foreach (var t in transactions.OrderBy(x => x.Date))
                Console.WriteLine(t.ToString());
        }

        public void ShowCategoryTree()
        {
            Console.WriteLine("\n--- Kategorie ---");
            CategoryTree.Root.Print();
        }
    }
}