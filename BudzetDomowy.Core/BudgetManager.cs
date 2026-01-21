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
    // Integruje wszystkie wzorce projektowe w jedną logikę biznesową.
    public class BudgetManager
    {
        private readonly double _monthlyLimit;
        private readonly double _initialBalance;

        private List<Transaction> transactions = new List<Transaction>();
        private List<IBudgetObserver> observers = new List<IBudgetObserver>();

        private ITransactionFactory _transactionFactory;
        private IForecastingStrategy _forecastingStrategy;

        public BudgetManager(double limit, double initialBalance, ITransactionFactory factory)
        {
            _monthlyLimit = limit;
            _initialBalance = initialBalance;
            _transactionFactory = factory;
            _forecastingStrategy = new AverageForecast();
        }

        public void AddTransaction(string type, double amount, string desc, DateTime date, string categoryName)
        {
            Transaction t = _transactionFactory.CreateTransaction(type, amount, desc, date, categoryName);
            transactions.Add(t);

            // Obsługa Composite (Drzewo)
            var categoryNode = CategoryTree.FindByName(categoryName);
            if (categoryNode is SingleCategory singleCat)
            {
                singleCat.AddAmount(amount);
                Console.WriteLine($"[System] Przypisano do kategorii: {categoryName}");
            }
            else
            {
                string fallbackCat = t is Expense ? "Inne Wydatki" : "Inne Przychody";
                var otherCat = CategoryTree.FindByName(fallbackCat) as SingleCategory;
                otherCat?.AddAmount(amount);
                Console.WriteLine($"[System] Kategoria '{categoryName}' nieznana. Przypisano do '{fallbackCat}'.");
            }

            Console.WriteLine($"[Manager] Dodano: {t.GetType().Name} | {t.Description}");

            // Observer Notify - wywołujemy przy każdej zmianie (lub tylko przy wydatku, zależy od preferencji)
            // Tutaj wywołujemy przy wydatku, żeby sprawdzić limity
            if (t is Expense)
            {
                Notify();
            }
        }

        public void Notify()
        {
            double balance = CalculateBalance();

            double totalExpenses = transactions.OfType<Expense>().Sum(t => t.Amount);

            // Przekazujemy 3 parametry: Saldo (do życia), Limit (cel), Wydatki (realizacja celu)
            foreach (var obs in observers)
            {
                obs.Update(balance, _monthlyLimit, totalExpenses);
            }
        }

        public double CalculateBalance()
        {
            double income = transactions.OfType<Income>().Sum(t => t.Amount);
            double expense = transactions.OfType<Expense>().Sum(t => t.Amount);
            return _initialBalance + income - expense;
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
            string stats = $"Liczba transakcji: {transactions.Count}, Saldo: {CalculateBalance():F2} PLN (Limit: {_monthlyLimit})";
            ReportDirector director = new ReportDirector(transactions, stats, "Raport Budżetowy");
            director.Construct(builder);
            return builder.GetReport();
        }

        public void ShowCurrentTransactions()
        {
            Console.WriteLine("\n--- Transakcje ---");
            foreach (var t in transactions.OrderBy(x => x.Date))
                Console.WriteLine(t.ToString());
        }

        public void ShowCategoryTree()
        {
            Console.WriteLine("\n--- STRUKTURA KATEGORII ---");
            CategoryTree.Root.Print();
        }
    }
}