using BudzetDomowy.Core.Models;
using BudzetDomowy.Core.Patterns.FactoryMethod;
using BudzetDomowy.Core.Patterns.ObserverMethod;
using BudzetDomowy.Core.Patterns.StrategyMethod;
using BudzetDomowy.Core.Patterns.BuilderMethod;
using BudzetDomowy.Core.Patterns.CompositeMethod;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BudzetDomowy.Core
{
    // Główny kontroler logiki biznesowej. Łączy wszystkie wzorce i zarządza stanem.
    public class BudgetManager
    {
        public double MonthlyLimit { get; private set; }
        public double InitialBalance { get; private set; }

        private List<Transaction> transactions = new List<Transaction>();
        private List<IBudgetObserver> observers = new List<IBudgetObserver>();

        // Pola do obsługi zapisu stanu drzewa kategorii
        private List<CustomCategoryDto> _customCategoriesList = new();
        private List<string> _deletedCategoriesList = new();

        private ITransactionFactory _transactionFactory;
        private IForecastingStrategy _forecastingStrategy;

        public BudgetManager(double limit, double initialBalance, ITransactionFactory factory)
        {
            MonthlyLimit = limit;
            InitialBalance = initialBalance;
            _transactionFactory = factory;
            _forecastingStrategy = new AverageForecast();
        }

        // Dodaje nową kategorię i zapisuje zmianę w pamięci trwałej
        public void AddNewCategory(string parentName, string newCategoryName, bool isGroup)
        {
            // 1. Dodaj do drzewa
            CategoryTree.AddCustomCategory(parentName, newCategoryName, isGroup);

            // 2. Dodaj do listy zapisu
            _customCategoriesList.Add(new CustomCategoryDto
            {
                ParentName = parentName,
                NewCategoryName = newCategoryName,
                IsGroup = isGroup
            });

            // Jeśli kategoria była wcześniej usunięta, usuń ją z listy usuniętych (przywrócenie)
            _deletedCategoriesList.RemoveAll(x => x.Equals(newCategoryName, StringComparison.OrdinalIgnoreCase));

            SaveData();
        }

        // Usuwa kategorię i zapisuje ten fakt.
        public void RemoveCategory(string name)
        {
            // 1. Usuń z drzewa
            CategoryTree.RemoveCategory(name);

            // 2. Aktualizuj persistence
            // Jeśli to była kategoria dodana przez użytkownika, po prostu usuń ją z listy 'custom'
            var customEntry = _customCategoriesList.FirstOrDefault(x => x.NewCategoryName.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (customEntry != null)
            {
                _customCategoriesList.Remove(customEntry);
            }
            else
            {
                // Jeśli to kategoria domyślna, musimy zapisać fakt jej usunięcia
                if (!_deletedCategoriesList.Contains(name))
                {
                    _deletedCategoriesList.Add(name);
                }
            }

            SaveData();
        }

        // Dodaje transakcję, aktualizuje drzewo, powiadamia obserwatorów i zapisuje stan.
        public void AddTransaction(string type, double amount, string desc, DateTime date, string categoryName)
        {
            Transaction t = _transactionFactory.CreateTransaction(type, amount, desc, date, categoryName);
            transactions.Add(t);
            UpdateTreeWithAmount(categoryName, amount);
            if (t is Expense) Notify();
            SaveData();
        }

        // Aktualizuje kwoty w strukturze Composite
        private void UpdateTreeWithAmount(string categoryName, double amount)
        {
            var categoryNode = CategoryTree.FindByName(categoryName);
            if (categoryNode is SingleCategory singleCat)
            {
                singleCat.AddAmount(amount);
            }
            else
            {
                // Fallback, jeśli kategoria nie istnieje (np. została usunięta)
                var fallback = CategoryTree.FindByName("Inne Wydatki") as SingleCategory;
                fallback?.AddAmount(amount);
            }
        }

        // Serializuje obecny stan do pliku JSON.
        public void SaveData()
        {
            var state = new AppState
            {
                Limit = MonthlyLimit,
                InitialBalance = InitialBalance,
                CustomCategories = _customCategoriesList,
                DeletedCategories = _deletedCategoriesList,
                Transactions = transactions.Select(t => new TransactionDto
                {
                    Type = t is Expense ? "wydatek" : "przychod",
                    Amount = t.Amount,
                    Description = t.Description,
                    Date = t.Date,
                    Category = t.Category
                }).ToList()
            };
            PersistenceManager.Save(state);
        }

        // Ładuje dane z pliku i odtwarza stan obiektów (drzewo, transakcje).
        public bool LoadData()
        {
            var state = PersistenceManager.Load();
            if (state == null) return false;

            MonthlyLimit = state.Limit;
            InitialBalance = state.InitialBalance;
            _customCategoriesList = state.CustomCategories;
            _deletedCategoriesList = state.DeletedCategories ?? new List<string>(); // null check dla starych plików

            // ODTWARZANIE DRZEWA
            CategoryTree.ResetToDefault();

            // 1. Dodaj customowe Kategorie
            foreach (var cat in _customCategoriesList)
            {
                try
                {
                    CategoryTree.AddCustomCategory(cat.ParentName, cat.NewCategoryName, cat.IsGroup);
                }
                catch { /* Ignoruj błędy */ }
            }

            // 2. Usuń skasowane
            foreach (var delName in _deletedCategoriesList)
            {
                try
                {
                    CategoryTree.RemoveCategory(delName);
                }
                catch { /* Ignoruj błędy */ }
            }

            // ODTWARZANIE TRANSAKCJI
            transactions.Clear();
            foreach (var dto in state.Transactions)
            {
                var t = _transactionFactory.CreateTransaction(dto.Type, dto.Amount, dto.Description, dto.Date, dto.Category);
                transactions.Add(t);
                UpdateTreeWithAmount(dto.Category, dto.Amount);
            }

            return true;
        }

        public void AddObserver(IBudgetObserver observer) { if (!observers.Contains(observer)) observers.Add(observer); }
        public void Notify()
        {
            double bal = CalculateBalance();
            double exp = transactions.OfType<Expense>().Sum(t => t.Amount);
            foreach (var o in observers) o.Update(bal, MonthlyLimit, exp);
        }
        public double CalculateBalance()
        {
            double inc = transactions.OfType<Income>().Sum(t => t.Amount);
            double exp = transactions.OfType<Expense>().Sum(t => t.Amount);
            return InitialBalance + inc - exp;
        }
        public void SetForecastingStrategy(IForecastingStrategy s) => _forecastingStrategy = s;
        public decimal GetForecast() => _forecastingStrategy.PredictNextMonth(transactions);
        public Report GenerateReport(IReportBuilder b)
        {
            string stats = $"Transakcje: {transactions.Count}, Saldo: {CalculateBalance():F2} PLN";
            var d = new ReportDirector(transactions, stats, "Raport");
            d.Construct(b);
            return b.GetReport();
        }
        public void ShowCurrentTransactions()
        {
            Console.WriteLine("\n--- Transakcje ---");
            foreach (var t in transactions.OrderBy(x => x.Date)) Console.WriteLine(t.ToString());
        }
        public void ShowCategoryTree() { Console.WriteLine("\n--- KATEGORIE ---"); CategoryTree.Root.Print(); }
    }
}