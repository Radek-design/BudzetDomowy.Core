using BudzetDomowy.Models;

namespace BudzetDomowy.Core.Patterns.BuilderMethod;

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
        if (reportBuilder == null)
            throw new ArgumentNullException(nameof(reportBuilder));

        reportBuilder.BuildHeader();
        reportBuilder.BuildTable(_transactions);
        reportBuilder.BuildSummary(_summary);
        reportBuilder.BuildFooter(_footer);
    }
}
