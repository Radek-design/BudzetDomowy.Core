using System;

namespace BudzetDomowy.Core.Models
{
    
    // Klasa bazowa reprezentująca pojedynczą operację finansową.
    // W kontekście wzorca Factory Method pełni rolę "Product".
    public abstract class Transaction
    {
        public double Amount { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }

        // Nazwa kategorii powiązana z drzewem kategorii (Composite).
        // Używamy luźnego powiązania (string), aby ułatwić zapis do CSV/PDF bez konieczności serializacji całych obiektów drzewa.
        
        public string Category { get; set; }

        protected Transaction(double amount, string description, DateTime date, string category)
        {
            Amount = amount;
            Description = description;
            Date = date;
            Category = category;
        }

        public override string ToString()
        {
            return $"{Date:yyyy-MM-dd} | [{Category}] {Description}: {Amount:F2} PLN";
        }
    }

    
    // Konkretna implementacja transakcji: Wydatek.
    // Wpływa negatywnie na saldo budżetu.
    public class Expense : Transaction
    {
        public Expense(double amount, string description, DateTime date, string category)
            : base(amount, description, date, category) { }
    }

    // Konkretna implementacja transakcji: Przychód.
    // Zwiększa saldo budżetu.
    public class Income : Transaction
    {
        public Income(double amount, string description, DateTime date, string category)
            : base(amount, description, date, category) { }
    }

    // Obiekt transferu danych (DTO) reprezentujący wygenerowany raport.
    // Przenosi gotową treść lub ścieżkę do pliku z warstwy Builder do warstwy prezentacji.
    public class Report(string header, string footer, string content)
    {
        public string header { get; set; } = header;
        public string footer { get; set; } = footer;
        public string content { get; set; } = content;
    }
}