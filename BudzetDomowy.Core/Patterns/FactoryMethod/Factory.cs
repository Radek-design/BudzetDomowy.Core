using BudzetDomowy.Core.Models;

namespace BudzetDomowy.Core.Patterns.FactoryMethod
{
    public interface ITransactionFactory
    {
        Transaction CreateTransaction(string type, double amount, string description, DateTime date);
    }

    public class StandardTransactionFactory : ITransactionFactory
    {
        public Transaction CreateTransaction(string type, double amount, string description, DateTime date)
        {
            switch (type.ToLower())
            {
                case "wydatek":
                    return new Expense(amount, description, date);
                case "przychod":
                case "przychód":
                    return new Income(amount, description, date);
                default:
                    throw new ArgumentException("Nieznany typ transakcji: " + type);
            }
        }
    }
}