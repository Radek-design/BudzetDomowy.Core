using System;

namespace BudzetDomowy.Core.Models
{
    public abstract class Transaction
    {
        
        // Klasa bazowa reprezentująca pojedynczą operację finansową.
        // Pełni rolę "Product" we wzorcu Factory Method.
        
        public double Amount { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }

        // Przechowujemy nazwę kategorii jako klucz do struktury Composite.
        // Decyzja projektowa: Luźne powiązanie (loose coupling) ułatwia serializację do CSV/PDF.
        public string Category { get; set; }

        public Transaction(double amount, string description, DateTime date, string category)
        {
            Amount = amount;
            Description = description;
            Date = date;
            Category = category;
        }

        public override string ToString()
        {
            return $"{Date:yyyy-MM-dd} | [{Category}] {Description}: {Amount} PLN";
        }
    }
    // Konkretny typ transakcji: Wydatek.
    public class Expense : Transaction
    {
        public Expense(double amount, string description, DateTime date, string category)
            : base(amount, description, date, category) { }
    }
    // Konkretny typ transakcji: Przychód.
    public class Income : Transaction
    {
        public Income(double amount, string description, DateTime date, string category)
            : base(amount, description, date, category) { }
    }
    // Klasa DTO (Data Transfer Object) przenosząca gotowy raport.
    public class Report(string header, string footer, string content)
    {
        public string header { get; set; } = header;
        public string footer { get; set; } = footer;
        // Content zawiera treść tekstową lub ścieżkę do wygenerowanego pliku
        public string content { get; set; } = content;
    }
}