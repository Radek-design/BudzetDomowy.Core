using BudzetDomowy.Models;
using BudzetDomowy.Patterns.Factory;

using System;
using System.Collections.Generic;
using System.Linq;

namespace BudzetDomowy
{
    public class BudgetManager
    {
        private double _monthlyLimit;
        private List<Transaction> _transactions = new List<Transaction>();

        private ITransactionFactory _transactionFactory;

        // Observer

        // Strategy
        

        public BudgetManager(double limit, ITransactionFactory factory)
        {
            _monthlyLimit = limit;
            _transactionFactory = factory;

        }

        public void AddTransaction(string type, double amount, string desc)
        {
            Transaction t = _transactionFactory.CreateTransaction(type, amount, desc);
            _transactions.Add(t);

            Console.WriteLine($" Utworzono i dodano: {t.GetType().Name}");

        }

        public double CalculateBalance()
        {
            double income = _transactions.OfType<Income>().Sum(t => t.Amount);
            double expense = _transactions.OfType<Expense>().Sum(t => t.Amount);
            return _monthlyLimit + income - expense;
        }
        public void ShowCurrentTransactions()
        {
            Console.WriteLine("\n--- Lista Transakcji ---");
            foreach (var t in _transactions)
            {
                Console.WriteLine(t.ToString());
            }
        }
    }
}