using System.Collections.Generic;

namespace BudzetDomowy.Core.Patterns.CompositeMethod
{
    // Abstrakcyjny komponent drzewa kategorii (Component).
    // Pozwala traktować pojedyncze kategorie i grupy kategorii w ten sam sposób.
    public abstract class CategoryComponent
    {
        public string Name { get; protected set; }

        protected CategoryComponent(string name)
        {
            Name = name;
        }
        // Metoda rekurencyjna, która musi być zaimplementowana przez dzieci
        public abstract double GetTotalAmount();

        // Metody do zarządzania strukturą (opcjonalne dla liści, stąd wirtualne rzucające wyjątek)
        public virtual void Add(CategoryComponent component)
        {
            throw new System.NotSupportedException();
        }

        public virtual void Remove(CategoryComponent component)
        {
            throw new System.NotSupportedException();
        }

        public abstract void Print(string indent = "");
    }
}