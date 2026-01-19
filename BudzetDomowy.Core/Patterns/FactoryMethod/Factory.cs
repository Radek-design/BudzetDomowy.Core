using BudzetDomowy.Models;
using System;

namespace BudzetDomowy.Patterns.Factory
{
    public interface ITransactionFactory
    {
        Transaction CreateTransaction(string type, double amount, string description);
    }

    public class StandardTransactionFactory : ITransactionFactory
    {
        public Transaction CreateTransaction(string type, double amount, string description)
        {
            switch (type.ToLower())
            {
                case "wydatek":
                    return new Expense(amount, description);
                case "przychod":
                case "przychód":
                    return new Income(amount, description);
                default:
                    throw new ArgumentException("Nieznany typ transakcji");
            }
        }
    }
}