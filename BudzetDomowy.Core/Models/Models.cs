using System;

namespace BudzetDomowy.Models
{
    public abstract class Transaction
    {
        public double Amount { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
 

        public Transaction(double amount, string description)
        {
            Amount = amount;
            Date = DateTime.Now;
            Description = description;
        }

        public override string ToString()
        {
            return $"{Date.ToShortDateString()} | {Description}: {Amount} PLN";
        }
    }

    public class Expense : Transaction
    {
        public Expense(double amount, string description) : base(amount, description) { }
    }

    public class Income : Transaction
    {
        public Income(double amount, string description) : base(amount, description) { }
    }

    public class Report(string header, string footer, string content)
    {
        public string header { get; set; } = header;
        public string footer { get; set; } = footer;
        public string content { get; set; } = content;
    }
}
