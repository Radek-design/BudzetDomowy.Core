using System.Collections.Generic;

namespace BudzetDomowy.Core.Patterns.CompositeMethod
{
    // Liść (Leaf) w strukturze drzewa.
    // Reprezentuje konkretną kategorię, która przechowuje kwoty.
    public class SingleCategory : CategoryComponent
    {
        private readonly List<double> _amounts = new();

        public SingleCategory(string name) : base(name) { }

        public void AddAmount(double amount)
        {
            _amounts.Add(amount);
        }

        // Suma dla liścia to suma jego własnych transakcji
        public override double GetTotalAmount()
        {
            double sum = 0;
            foreach (var a in _amounts)
                sum += a;
            return sum;
        }

        public override void Print(string indent = "")
        {
            Console.WriteLine($"{indent}- {Name}: {GetTotalAmount():0.00} PLN");
        }
    }
}