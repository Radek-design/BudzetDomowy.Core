using BudzetDomowy.Core.Models;

namespace BudzetDomowy.Core.Patterns.FactoryMethod
{
    // Interfejs Fabryki (Creator).
    // Definiuje kontrakt tworzenia transakcji, separując logikę tworzenia obiektów od logiki biznesowej menedżera.
    public interface ITransactionFactory
    {
        Transaction CreateTransaction(string type, double amount, string description, DateTime date, string category);
    }

    // Standardowa implementacja fabryki transakcji.
    // Odpowiada za decyzję, którą klasę (Income czy Expense) instancjonować na podstawie tekstu.
    public class StandardTransactionFactory : ITransactionFactory
    {
        // Tworzy odpowiedni obiekt transakcji na podstawie podanego typu.
        // <param name="type">Typ transakcji ("wydatek" lub "przychod")</param>
        public Transaction CreateTransaction(string type, double amount, string description, DateTime date, string category)
        {
            // Zastosowanie switch expression dla czytelności i uniknięcia wielopiętrowych if-ów.
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