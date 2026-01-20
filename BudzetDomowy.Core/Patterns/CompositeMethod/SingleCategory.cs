using System;
using System.Collections.Generic;

namespace BudzetDomowy.Core.Patterns.CompositeMethod
{
    // Liść (Leaf) - Pojedyncza Kategoria (np. "Czynsz").
    // Nie posiada dzieci, przechowuje konkretne wartości.
    public class SingleCategory : CategoryComponent
    {
        private readonly List<double> _amounts = new();

        public SingleCategory(string name) : base(name) { }

        public void AddAmount(double amount) => _amounts.Add(amount);

        public override double GetTotalAmount()
        {
            double sum = 0;
            foreach (var a in _amounts) sum += a;
            return sum;
        }

        public override void Print(string indent = "")
        {
            Console.WriteLine($"{indent}- {Name}: {GetTotalAmount():0.00} PLN");
        }
    }
}