using System.Text;
using BudzetDomowy.Core.Models;
using System.IO;

namespace BudzetDomowy.Core.Patterns.BuilderMethod;

// Budowniczy raportów CSV.
// Generuje plik tekstowy z danymi oddzielonymi przecinkami, gotowy do importu w Excelu.
public class CsvReportBuilder : IReportBuilder
{
    private StringBuilder _sb = new StringBuilder();
    private string _header = "";
    private string _footer = "";
    private string _summary = "";

    public IReportBuilder BuildHeader()
    {
        _header = "Data,Kategoria,Opis,Kwota,Typ";
        return this;
    }

    public IReportBuilder BuildTable(List<Transaction> transactions)
    {
        _sb.AppendLine(_header);
        foreach (var t in transactions)
        {
            string type = t is Expense ? "Wydatek" : "Przychod";
            // Zabezpieczenie przed uszkodzeniem CSV przecinkami w opisie
            string safeDesc = t.Description.Contains(",") ? $"\"{t.Description}\"" : t.Description;
            _sb.AppendLine($"{t.Date:yyyy-MM-dd},{t.Category},{safeDesc},{t.Amount},{type}");
        }
        return this;
    }

    public IReportBuilder BuildSummary(string stats)
    {
        _summary = stats;
        _sb.AppendLine($"\nSUMMARY,,,,{stats}");
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
        File.WriteAllText(fileName, _sb.ToString());
        return new Report("CSV Export", _footer, $"WYGENEROWANO PLIK CSV:\n{Path.GetFullPath(fileName)}");
    }
}