using System;
using System.Collections.Generic;

namespace BudzetDomowy.Core.Patterns.CompositeMethod
{
    // Kompozyt (Composite) w strukturze drzewa.
    // Przechowuje listę dzieci (innych grup lub kategorii) i deleguje do nich operacje.
    public class CategoryGroup : CategoryComponent
    {
        public IReadOnlyList<CategoryComponent> GetChildren() => _children;

        private readonly List<CategoryComponent> _children = new();

        public CategoryGroup(string name) : base(name) { }

        public override void Add(CategoryComponent component)
        {
            _children.Add(component);
        }

        public override void Remove(CategoryComponent component)
        {
            _children.Remove(component);
        }

        // Rekurencja: Suma grupy to suma wszystkich jej dzieci
        public override double GetTotalAmount()
        {
            double sum = 0;
            foreach (var child in _children)
                sum += child.GetTotalAmount();
            return sum;
        }

        public override void Print(string indent = "")
        {
            Console.WriteLine($"{indent}+ {Name}: {GetTotalAmount():0.00} PLN");
            foreach (var child in _children)
                child.Print(indent + "  ");
        }
    }
}