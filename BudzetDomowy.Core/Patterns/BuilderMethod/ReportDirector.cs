using BudzetDomowy.Core.Models;
using System;

namespace BudzetDomowy.Core.Patterns.BuilderMethod;

// Klasa Dyrektora (Director).
// Odpowiada za sekwencję kroków budowania raportu. Nie zna szczegółów implementacji formatów.
public class ReportDirector
{
    private readonly List<Transaction> _transactions;
    private readonly string _summary;
    private readonly string _footer;

    public ReportDirector(List<Transaction> transactions, string summary, string footer)
    {
        _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
        _summary = summary;
        _footer = footer;
    }

    public void Construct(IReportBuilder reportBuilder)
    {
        if (reportBuilder == null) throw new ArgumentNullException(nameof(reportBuilder));

        // Ustala kolejność sekcji w raporcie
        reportBuilder.BuildHeader();
        reportBuilder.BuildTable(_transactions);
        reportBuilder.BuildSummary(_summary);
        reportBuilder.BuildFooter(_footer);
    }
}