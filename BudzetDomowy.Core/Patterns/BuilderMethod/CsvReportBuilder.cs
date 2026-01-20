using System.Text;
using BudzetDomowy.Models;

namespace BudzetDomowy.Core.Patterns.BuilderMethod;

public class CsvReportBuilder : IReportBuilder
{
    private StringBuilder _contentBuilder = new();
    private string _header = string.Empty;
    private string _footer = string.Empty;

    public IReportBuilder BuildHeader()
    {
        _header = "Date,Description,Amount";
        return this;
    }

    public IReportBuilder BuildTable(List<Transaction> transactions)
    {
        if (transactions == null)
            throw new ArgumentNullException(nameof(transactions));

        foreach (var t in transactions)
        {
            _contentBuilder.AppendLine(
                $"{t.Date:yyyy-MM-dd},{Escape(t.Description)},{t.Amount}");
        }
        return this;
    }

    public IReportBuilder BuildSummary(string stats)
    {
        if (string.IsNullOrWhiteSpace(stats))
            return this;

        _contentBuilder.AppendLine();
        _contentBuilder.AppendLine($"Summary,{Escape(stats)}");
        return this;
    }

    public IReportBuilder BuildFooter(string footer)
    {
        _footer = footer ?? string.Empty;
        return this;
    }

    public Report GetReport()
    {
        return new Report(
            header: _header,
            footer: _footer,
            content: _contentBuilder.ToString()
        );
    }

    private static string Escape(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Contains(',') || value.Contains('"'))
        {
            value = value.Replace("\"", "\"\"");
            return $"\"{value}\"";
        }

        return value;
    }
}
