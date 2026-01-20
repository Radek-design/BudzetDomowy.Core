using BudzetDomowy.Core.Models;

namespace BudzetDomowy.Core.Patterns.FactoryMethod
{
    public interface ITransactionFactory
    {
        // Dodano parametr string category
        Transaction CreateTransaction(string type, double amount, string description, DateTime date, string category);
    }

    public class StandardTransactionFactory : ITransactionFactory
    {
        public Transaction CreateTransaction(string type, double amount, string description, DateTime date, string category)
        {
            switch (type.ToLower())
            {
                case "wydatek":
                    return new Expense(amount, description, date, category);
                case "przychod":
                case "przychód":
                    return new Income(amount, description, date, category);
                default:
                    throw new ArgumentException("Nieznany typ transakcji: " + type);
            }
        }
    }
}