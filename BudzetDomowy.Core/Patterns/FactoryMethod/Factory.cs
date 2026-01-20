using BudzetDomowy.Core.Models;

namespace BudzetDomowy.Core.Patterns.FactoryMethod
{
    // Interfejs "Creator" we wzorcu Factory Method.
    // Definiuje kontrakt tworzenia transakcji bez ujawniania logiki instancjonowania.
    public interface ITransactionFactory
    {
        Transaction CreateTransaction(string type, double amount, string description, DateTime date, string category);
    }

    // Konkretna implementacja fabryki.
    // Centralizuje logikę decyzyjną dotyczącą tego, jaką klasę transakcji utworzyć.
    public class StandardTransactionFactory : ITransactionFactory
    {
        public Transaction CreateTransaction(string type, double amount, string description, DateTime date, string category)
        {
            // Prosty switch zamiast rozbudowanych if-ów w głównej logice programu.
            // Zapewnia łatwą rozszerzalność o nowe typy transakcji.
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