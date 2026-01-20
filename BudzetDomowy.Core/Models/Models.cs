using System;
using BudzetDomowy.Core.Patterns.CompositeMethod;

namespace BudzetDomowy.Core.Models
{
    public abstract class Transaction
    {
        public double Amount { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }

        // Composite (UML)
        public CategoryComponent? Category { get; set; }

        // Konstruktor z mastera (dla Factory)
        protected Transaction(double amount, string description)
        {
            Amount = amount;
            Date = DateTime.Now;
            Description = description;
            Category = null;
        }

        // Konstruktor z Twojej wersji (dla Composite)
        protected Transaction(double amount, string description, CategoryComponent category)
        {
            Amount = amount;
            Date = DateTime.Now;
            Description = description;
            Category = category;
        }

        public override string ToString()
        {
            var catName = Category?.Name ?? "BrakKategorii";
            return $"{Date.ToShortDateString()} | [{catName}] {Description}: {Amount} PLN";
        }
    }

    public class Expense : Transaction
    {
        // z mastera (Factory)
        public Expense(double amount, string description) : base(amount, description) { }

        // Twoje (Composite)
        public Expense(double amount, string description, CategoryComponent category)
            : base(amount, description, category) { }
    }

    public class Income : Transaction
    {
        // z mastera (Factory)
        public Income(double amount, string description) : base(amount, description) { }

        // Twoje (Composite)
        public Income(double amount, string description, CategoryComponent category)
            : base(amount, description, category) { }
    }

    
    public class Report(string header, string footer, string content)
    {
        public string header { get; set; } = header;
        public string footer { get; set; } = footer;
        public string content { get; set; } = content;
    }
}
