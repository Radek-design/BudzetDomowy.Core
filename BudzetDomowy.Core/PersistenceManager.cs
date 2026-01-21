using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using BudzetDomowy.Core.Models;

namespace BudzetDomowy.Core
{
    // Klasa reprezentująca stan aplikacji do zapisu w pliku JSON.
    // Służy jako kontener na wszystkie dane, które muszą przetrwać restart programu.
    public class AppState
    {
        public double Limit { get; set; }
        public double InitialBalance { get; set; }
        public List<TransactionDto> Transactions { get; set; } = new();

        // Lista kategorii dodanych ręcznie przez użytkownika
        public List<CustomCategoryDto> CustomCategories { get; set; } = new();

        // Lista nazw kategorii usuniętych przez użytkownika (aby nie przywracać ich z domyślnego drzewa)
        public List<string> DeletedCategories { get; set; } = new();
    }

    // Prosty obiekt transferu danych (DTO) dla transakcji.
    public class TransactionDto
    {
        public string Type { get; set; }
        public double Amount { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public string Category { get; set; }
    }

    // DTO dla niestandardowej kategorii lub grupy.
    public class CustomCategoryDto
    {
        public string ParentName { get; set; }
        public string NewCategoryName { get; set; }

        // Flaga czy to grupa (np. Samochód) czy kategoria (np. Paliwo)
        public bool IsGroup { get; set; }
    }

    public static class PersistenceManager
    {
        private const string FileName = "budget_data.json";

        // Zapisuje stan aplikacji do pliku w formacie JSON.
        public static void Save(AppState state)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(state, options);
            File.WriteAllText(FileName, jsonString);
        }

        // Odczytuje stan aplikacji z pliku. Zwraca null, jeśli plik nie istnieje.
        public static AppState? Load()
        {
            if (!File.Exists(FileName)) return null;
            try
            {
                string jsonString = File.ReadAllText(FileName);
                return JsonSerializer.Deserialize<AppState>(jsonString);
            }
            catch
            {
                return null;
            }
        }
    }
}