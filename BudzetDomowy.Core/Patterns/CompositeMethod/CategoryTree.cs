using System;

namespace BudzetDomowy.Core.Patterns.CompositeMethod
{
    // Klasa pomocnicza inicjalizująca strukturę drzewa kategorii.
    // Symuluje bazę danych dostępnych kategorii.
    public static class CategoryTree
    {
        public static CategoryGroup Root { get; } = BuildDefaultTree();

        public static CategoryComponent? FindByName(string name) => FindRecursive(Root, name);

        private static CategoryGroup BuildDefaultTree()
        {
            var root = new CategoryGroup("Wszystko");

            var dom = new CategoryGroup("Dom");
            dom.Add(new SingleCategory("Czynsz"));
            dom.Add(new SingleCategory("Prąd"));
            dom.Add(new SingleCategory("Internet"));

            var jedzenie = new CategoryGroup("Jedzenie");
            jedzenie.Add(new SingleCategory("Sklep"));
            jedzenie.Add(new SingleCategory("Restauracje"));

            root.Add(dom);
            root.Add(jedzenie);
            root.Add(new SingleCategory("Transport"));
            root.Add(new SingleCategory("Inne"));

            return root;
        }

        private static CategoryComponent? FindRecursive(CategoryComponent node, string name)
        {
            if (node.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase)) return node;

            if (node is CategoryGroup group)
            {
                foreach (var child in group.GetChildren())
                {
                    var found = FindRecursive(child, name);
                    if (found != null) return found;
                }
            }
            return null;
        }
    }
}