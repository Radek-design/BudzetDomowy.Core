using System.Collections.Generic;

namespace BudzetDomowy.Core.Patterns.CompositeMethod
{
    public abstract class CategoryComponent
    {
        public string Name { get; protected set; }

        protected CategoryComponent(string name)
        {
            Name = name;
        }

        public abstract double GetTotalAmount();

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