using System;
using System.Linq;

namespace BudzetDomowy.Core.Patterns.CompositeMethod
{
    public static class CategoryTree
    {
        public static CategoryGroup Root { get; } = BuildDefaultTree();

        // Metoda do wyszukiwania dowolnej kategorii (rekurencyjnie)
        public static CategoryComponent? FindByName(string name)
        {
            return FindRecursive(Root, name);
        }

        // Metoda pomocnicza do pobierania głównej gałęzi (np. "WYDATKI")
        public static CategoryGroup? GetGroupByName(string name)
        {
            return Root.GetChildren()
                .OfType<CategoryGroup>()
                .FirstOrDefault(g => g.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        private static CategoryGroup BuildDefaultTree()
        {
            var root = new CategoryGroup("SALDO CAŁKOWITE");

            // --- GAŁĄŹ 1: PRZYCHODY ---
            var przychody = new CategoryGroup("PRZYCHODY");
            przychody.Add(new SingleCategory("Wypłata"));
            przychody.Add(new SingleCategory("Premia"));
            przychody.Add(new SingleCategory("Zwrot podatku"));
            przychody.Add(new SingleCategory("Sprzedaż"));
            przychody.Add(new SingleCategory("Inne Przychody"));

            // --- GAŁĄŹ 2: WYDATKI ---
            var wydatki = new CategoryGroup("WYDATKI");

            var dom = new CategoryGroup("Dom");
            dom.Add(new SingleCategory("Czynsz"));
            dom.Add(new SingleCategory("Prąd"));
            dom.Add(new SingleCategory("Internet"));
            dom.Add(new SingleCategory("Woda"));

            var jedzenie = new CategoryGroup("Jedzenie");
            jedzenie.Add(new SingleCategory("Sklep"));
            jedzenie.Add(new SingleCategory("Restauracje"));
            jedzenie.Add(new SingleCategory("Dieta"));

            var transport = new CategoryGroup("Transport");
            transport.Add(new SingleCategory("Paliwo"));
            transport.Add(new SingleCategory("Bilety"));
            transport.Add(new SingleCategory("Uber/Bolt"));

            var rozrywka = new CategoryGroup("Rozrywka");
            rozrywka.Add(new SingleCategory("Kino"));
            rozrywka.Add(new SingleCategory("Gry"));
            rozrywka.Add(new SingleCategory("Sport"));

            // Składamy drzewo wydatków
            wydatki.Add(dom);
            wydatki.Add(jedzenie);
            wydatki.Add(transport);
            wydatki.Add(rozrywka);
            wydatki.Add(new SingleCategory("Inne Wydatki"));

            // Dodajemy główne gałęzie do korzenia
            root.Add(przychody);
            root.Add(wydatki);

            return root;
        }

        private static CategoryComponent? FindRecursive(CategoryComponent node, string name)
        {
            if (node.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
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