using System;

namespace BudzetDomowy.Core.Patterns.CompositeMethod
{
    public static class CategoryTree
    {
        public static CategoryGroup Root { get; } = BuildDefaultTree();

        public static CategoryComponent? FindByName(string name)
        {
            return FindRecursive(Root, name);
        }

        private static CategoryGroup BuildDefaultTree()
        {
            // Korzeń główny
            var root = new CategoryGroup("SALDO CAŁKOWITE (Suma przepływów)");

            // --- GAŁĄŹ 1: PRZYCHODY ---
            var przychody = new CategoryGroup("PRZYCHODY");
            przychody.Add(new SingleCategory("Wypłata"));
            przychody.Add(new SingleCategory("Premia"));
            przychody.Add(new SingleCategory("Inne Przychody"));

            // --- GAŁĄŹ 2: WYDATKI ---
            var wydatki = new CategoryGroup("WYDATKI");

            var dom = new CategoryGroup("Dom");
            dom.Add(new SingleCategory("Czynsz"));
            dom.Add(new SingleCategory("Prąd"));
            dom.Add(new SingleCategory("Internet"));

            var jedzenie = new CategoryGroup("Jedzenie");
            jedzenie.Add(new SingleCategory("Sklep"));
            jedzenie.Add(new SingleCategory("Restauracje"));

            var transport = new CategoryGroup("Transport");
            transport.Add(new SingleCategory("Paliwo"));
            transport.Add(new SingleCategory("Bilety"));

            // Składamy drzewo wydatków
            wydatki.Add(dom);
            wydatki.Add(jedzenie);
            wydatki.Add(transport);
            wydatki.Add(new SingleCategory("Inne Wydatki"));

            // Dodajemy główne gałęzie do korzenia
            root.Add(przychody);
            root.Add(wydatki);

            return root;
        }

        private static CategoryComponent? FindRecursive(CategoryComponent node, string name)
        {
            // Ignorujemy wielkość liter przy szukaniu
            if (node.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
                return node;

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