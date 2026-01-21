using System;
using System.Linq;

namespace BudzetDomowy.Core.Patterns.CompositeMethod
{
    // Statyczna klasa zarządzająca strukturą kategorii (Wzorzec Composite).
    public static class CategoryTree
    {
        public static CategoryGroup Root { get; private set; } = BuildDefaultTree();

        // Wyszukuje rekurencyjnie komponent (grupę lub liść) o podanej nazwie.
        public static CategoryComponent? FindByName(string name)
        {
            return FindRecursive(Root, name);
        }

        public static CategoryGroup? GetGroupByName(string name)
        {
            return Root.GetChildren()
                .OfType<CategoryGroup>()
                .FirstOrDefault(g => g.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        /// Dodaje nową kategorię LUB grupę.
        public static void AddCustomCategory(string parentName, string newCategoryName, bool isGroup)
        {
            var parentNode = FindByName(parentName);

            if (parentNode is CategoryGroup group)
            {
                if (group.GetChildren().Any(c => c.Name.Equals(newCategoryName, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new Exception($"Element '{newCategoryName}' już istnieje w grupie '{parentName}'.");
                }

                if (isGroup)
                {
                    group.Add(new CategoryGroup(newCategoryName));
                }
                else
                {
                    group.Add(new SingleCategory(newCategoryName));
                }
            }
            else if (parentNode is SingleCategory)
            {
                throw new Exception($"Nie można dodać elementu do '{parentName}', ponieważ to jest kategoria końcowa, a nie grupa.");
            }
            else
            {
                throw new Exception($"Nie znaleziono grupy nadrzędnej '{parentName}'.");
            }
        }

        // Usuwa kategorię o podanej nazwie z drzewa.
        public static void RemoveCategory(string name)
        {
            // Zabezpieczenie przed usunięciem kluczowych gałęzi
            string upperName = name.ToUpper();
            if (upperName == "PRZYCHODY" || upperName == "WYDATKI" || upperName == "SALDO CAŁKOWITE")
            {
                throw new Exception("Nie można usunąć systemowych kategorii głównych (PRZYCHODY/WYDATKI).");
            }

            // Znajdź rodzica usuwanego elementu
            var parent = FindParentOf(Root, name);

            if (parent == null)
            {
                throw new Exception($"Nie znaleziono kategorii '{name}' lub jest ona korzeniem.");
            }

            // Znajdź sam element i go usuń
            var childToRemove = parent.GetChildren()
                .FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (childToRemove != null)
            {
                parent.Remove(childToRemove);
            }
        }

        // Metoda pomocnicza do szukania rodzica
        private static CategoryGroup? FindParentOf(CategoryGroup currentGroup, string childName)
        {
            foreach (var child in currentGroup.GetChildren())
            {
                // Sprawdź czy to bezpośrednie dziecko
                if (child.Name.Equals(childName, StringComparison.OrdinalIgnoreCase))
                {
                    return currentGroup;
                }

                // Jeśli dziecko jest grupą, szukaj głębiej
                if (child is CategoryGroup subGroup)
                {
                    var result = FindParentOf(subGroup, childName);
                    if (result != null) return result;
                }
            }
            return null;
        }

        // Przywraca drzewo do ustawień fabrycznych (używane przy ładowaniu zapisu).
        public static void ResetToDefault()
        {
            Root = BuildDefaultTree();
        }

        // Buduje startową strukturę drzewa
        private static CategoryGroup BuildDefaultTree()
        {
            var root = new CategoryGroup("SALDO CAŁKOWITE");

            var przychody = new CategoryGroup("PRZYCHODY");
            przychody.Add(new SingleCategory("Wypłata"));
            przychody.Add(new SingleCategory("Premia"));
            przychody.Add(new SingleCategory("Inne Przychody"));

            var wydatki = new CategoryGroup("WYDATKI");

            var dom = new CategoryGroup("Dom");
            dom.Add(new SingleCategory("Czynsz"));
            dom.Add(new SingleCategory("Prąd"));
            dom.Add(new SingleCategory("Internet"));

            var jedzenie = new CategoryGroup("Jedzenie");
            jedzenie.Add(new SingleCategory("Sklep"));
            jedzenie.Add(new SingleCategory("Restauracje"));

            var rozrywka = new CategoryGroup("Rozrywka");
            rozrywka.Add(new SingleCategory("Kino"));

            var inne = new CategoryGroup("Inne Wydatki");

            wydatki.Add(dom);
            wydatki.Add(jedzenie);
            wydatki.Add(rozrywka);
            wydatki.Add(inne);

            root.Add(przychody);
            root.Add(wydatki);

            return root;
        }

        private static CategoryComponent? FindRecursive(CategoryComponent node, string name)
        {
            if (node.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) return node;
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