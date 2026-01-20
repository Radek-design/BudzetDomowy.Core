using BudzetDomowy.Core.Models;
using BudzetDomowy.Core.Patterns.FactoryMethod;
using BudzetDomowy.Core.Patterns.ObserverMethod;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BudzetDomowy
{
    public class BudgetManager
    {
        private double MonthlyLimit;
        private List<Transaction> transactions = new List<Transaction>();

        // Observer (UML)
        private List<IBudgetObserver> observers = new List<IBudgetObserver>();

        private ITransactionFactory _transactionFactory;

        public BudgetManager(double limit, ITransactionFactory factory)
        {
            MonthlyLimit = limit;
            _transactionFactory = factory;
        }

        public void AddTransaction(string type, double amount, string desc)
        {
            Transaction t = _transactionFactory.CreateTransaction(type, amount, desc);
            transactions.Add(t);

            Console.WriteLine($" Utworzono i dodano: {t.GetType().Name}");

            // wg prezentacji: notify na wydatku
            if (t is Expense)
            {
                Notify();
            }
        }

        // UML: Notify()
        public void Notify()
        {
            double balance = CalculateBalance();
            foreach (var obs in observers)
            {
                obs.Update(balance, MonthlyLimit);
            }
        }

        public void AddObserver(IBudgetObserver observer)
        {
            if (!observers.Contains(observer))
                observers.Add(observer);
        }

        public double CalculateBalance()
        {
            double income = transactions.OfType<Income>().Sum(t => t.Amount);
            double expense = transactions.OfType<Expense>().Sum(t => t.Amount);
            return MonthlyLimit + income - expense;
        }

        public void ShowCurrentTransactions()
        {
            Console.WriteLine("\n--- Lista Transakcji ---");
            foreach (var t in transactions)
                Console.WriteLine(t.ToString());
        }
    }
}
