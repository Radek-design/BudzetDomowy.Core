using System.Text;
using BudzetDomowy.Core.Models;
using System.IO;

namespace BudzetDomowy.Core.Patterns.BuilderMethod;

// Konkretny Budowniczy dla formatu CSV.
// Tworzy prosty plik tekstowy oddzielany przecinkami, idealny do Excela.

public class CsvReportBuilder : IReportBuilder
{
    private StringBuilder _sb = new StringBuilder();
    private string _header = "";
    private string _footer = "";
    private string _summary = "";

    public IReportBuilder BuildHeader()
    {
        // W CSV nagłówek to zazwyczaj nazwy kolumn
        _header = "Data,Kategoria,Opis,Kwota,Typ";
        return this;
    }

    public IReportBuilder BuildTable(List<Transaction> transactions)
    {
        // Najpierw dodajemy nagłówek kolumn do bufora
        _sb.AppendLine(_header);

        foreach (var t in transactions)
        {
            string type = t is Expense ? "Wydatek" : "Przychod";
            // Format CSV: wartości oddzielone przecinkami
            // Escape'owanie: jeśli opis ma przecinek, dajemy go w cudzysłowie
            string safeDesc = t.Description.Contains(",") ? $"\"{t.Description}\"" : t.Description;

            _sb.AppendLine($"{t.Date:yyyy-MM-dd},{t.Category},{safeDesc},{t.Amount},{type}");
        }
        return this;
    }

    public IReportBuilder BuildSummary(string stats)
    {
        _summary = stats;
        _sb.AppendLine();
        _sb.AppendLine("PODSUMOWANIE,,,,");
        // Zakładamy, że stats nie ma przecinków psujących CSV, albo dajemy w cudzysłów
        _sb.AppendLine($",,,,{stats}");
        return this;
    }

    public IReportBuilder BuildFooter(string footer)
    {
        _footer = footer;
        _sb.AppendLine($",,,,{footer}");
        return this;
    }

    public Report GetReport()
    {
        string fileName = $"Dane_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

        // Zapis fizyczny na dysku
        File.WriteAllText(fileName, _sb.ToString());

        return new Report(
            "CSV Export",
            _footer,
            $"WYGENEROWANO PLIK CSV:\n{Path.GetFullPath(fileName)}"
        );
    }
}