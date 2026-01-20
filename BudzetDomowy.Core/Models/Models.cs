namespace BudzetDomowy.Core.Models
{
    public abstract class Transaction
    {
        public double Amount { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }

        // Dodano parametr DateTime date
        public Transaction(double amount, string description, DateTime date)
        {
            Amount = amount;
            Description = description;
            Date = date;
        }

        public override string ToString()
        {
            return $"{Date:yyyy-MM-dd} | {Description}: {Amount} PLN";
        }
    }

    public class Expense : Transaction
    {
        public Expense(double amount, string description, DateTime date)
            : base(amount, description, date) { }
    }

    public class Income : Transaction
    {
        public Income(double amount, string description, DateTime date)
            : base(amount, description, date) { }
    }

    public class Report(string header, string footer, string content)
    {
        public string header { get; set; } = header;
        public string footer { get; set; } = footer;
        public string content { get; set; } = content;
    }
}