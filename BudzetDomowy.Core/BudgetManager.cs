using BudzetDomowy.Core.Models;
using BudzetDomowy.Core.Patterns.FactoryMethod;
using BudzetDomowy.Core.Patterns.ObserverMethod;
using BudzetDomowy.Core.Patterns.StrategyMethod;
using BudzetDomowy.Core.Patterns.BuilderMethod;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BudzetDomowy
{
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
            _forecastingStrategy = new AverageForecast();
        }

        // ZMIANA: Dodano parametr DateTime date
        public void AddTransaction(string type, double amount, string desc, DateTime date)
        {
            Transaction t = _transactionFactory.CreateTransaction(type, amount, desc, date);
            transactions.Add(t);

            Console.WriteLine($"[Manager] Dodano: {t.GetType().Name} z datą {t.Date:yyyy-MM-dd}");

            if (t is Expense)
            {
                Notify();
            }
        }

        public void AddObserver(IBudgetObserver observer)
        {
            if (!observers.Contains(observer))
                observers.Add(observer);
        }

        public void Notify()
        {
            double balance = CalculateBalance();
            foreach (var obs in observers)
            {
                obs.Update(balance, MonthlyLimit);
            }
        }

        public void SetForecastingStrategy(IForecastingStrategy strategy)
        {
            _forecastingStrategy = strategy;
            Console.WriteLine($"[Manager] Zmieniono strategię na: {strategy.GetType().Name}");
        }

        public decimal GetForecast()
        {
            if (_forecastingStrategy == null)
                throw new InvalidOperationException("Nie ustawiono strategii.");
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
            // Sortujemy po dacie, żeby było czytelniej
            foreach (var t in transactions.OrderBy(x => x.Date))
                Console.WriteLine(t.ToString());
        }
    }
}