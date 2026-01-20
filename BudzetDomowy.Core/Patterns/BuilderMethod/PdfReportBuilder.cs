using System.Text;
using BudzetDomowy.Core.Models;

namespace BudzetDomowy.Core.Patterns.BuilderMethod;

public class PdfReportBuilder : IReportBuilder
{
    private StringBuilder _contentBuilder = new();
    private string _header = string.Empty;
    private string _footer = string.Empty;

    public IReportBuilder BuildHeader()
    {
        _header = "==== BUDGET REPORT ====";
        _contentBuilder.AppendLine(_header);
        _contentBuilder.AppendLine();
        return this;
    }

    public IReportBuilder BuildTable(List<Transaction> transactions)
    {
        if (transactions == null)
            throw new ArgumentNullException(nameof(transactions));

        _contentBuilder.AppendLine("Date       | Description                 | Amount");
        _contentBuilder.AppendLine("------------------------------------------------");

        foreach (var t in transactions)
        {
            string date = t.Date.ToString("yyyy-MM-dd");
            string desc = t.Description.PadRight(25).Substring(0, Math.Min(25, t.Description.Length));
            string amount = t.Amount.ToString("F2").PadLeft(10);

            _contentBuilder.AppendLine($"{date} | {desc} | {amount}");
        }

        _contentBuilder.AppendLine();
        return this;
    }

    public IReportBuilder BuildSummary(string stats)
    {
        if (string.IsNullOrWhiteSpace(stats))
            return this;

        _contentBuilder.AppendLine("---- SUMMARY ----");
        _contentBuilder.AppendLine(stats);
        _contentBuilder.AppendLine();
        return this;
    }

    public IReportBuilder BuildFooter(string footer)
    {
        _footer = footer ?? string.Empty;
        _contentBuilder.AppendLine(_footer);
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
}
