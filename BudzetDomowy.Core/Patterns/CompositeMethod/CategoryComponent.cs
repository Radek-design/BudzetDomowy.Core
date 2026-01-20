namespace BudzetDomowy.Core.Patterns.CompositeMethod
{
    // Abstrakcyjny komponent drzewa kategorii (wzorzec Composite).
    // Dzięki niemu klient traktuje pojedyncze kategorie i grupy kategorii w ten sam sposób.
    public abstract class CategoryComponent
    {
        public string Name { get; protected set; }

        protected CategoryComponent(string name) => Name = name;

        // Metoda operacyjna wspólna dla liścia i kompozytu
        public abstract double GetTotalAmount();

        // Metody zarządzania dziećmi (opcjonalne dla liścia, stąd domyślny wyjątek)
        public virtual void Add(CategoryComponent component) => throw new System.NotSupportedException();
        public virtual void Remove(CategoryComponent component) => throw new System.NotSupportedException();
        public abstract void Print(string indent = "");
    }
}