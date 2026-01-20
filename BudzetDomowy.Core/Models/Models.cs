using BudzetDomowy.Core.Patterns.CompositeMethod;

namespace BudzetDomowy.Core.Models
{
    public abstract class Transaction
    {
        public double Amount { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public CategoryComponent Category { get; set; }


        public Transaction(double amount, string description, CategoryComponent category)
        {
            Amount = amount;
            Date = DateTime.Now;
            Description = description;
            Category = category;
        }


        public override string ToString()
        {
            return $"{Date.ToShortDateString()} | [{Category?.Name}] {Description}: {Amount} PLN";
        }

    }

    public class Expense : Transaction
    {
        public Expense(double amount, string description, CategoryComponent category) 
            : base(amount, description, category) { }
    }

    public class Income : Transaction
    {
        public Income(double amount, string description, CategoryComponent category) 
            : base(amount, description, category) { }
    }

}