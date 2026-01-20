using System;

namespace BudzetDomowy.Core.Models
{
    public abstract class Transaction
    {
        public double Amount { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        // NOWOŚĆ: Przechowujemy nazwę kategorii
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

    public class Expense : Transaction
    {
        public Expense(double amount, string description, DateTime date, string category)
            : base(amount, description, date, category) { }
    }

    public class Income : Transaction
    {
        public Income(double amount, string description, DateTime date, string category)
            : base(amount, description, date, category) { }
    }

    public class Report(string header, string footer, string content)
    {
        public string header { get; set; } = header;
        public string footer { get; set; } = footer;
        public string content { get; set; } = content;
    }
}