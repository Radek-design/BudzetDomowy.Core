using BudzetDomowy.Core.Models;

namespace BudzetDomowy.Core.Patterns.BuilderMethod;

public interface IReportBuilder
{
    public IReportBuilder BuildHeader();
    public IReportBuilder BuildTable(List<Transaction> transactions);
    public IReportBuilder BuildFooter(string footer);
    public IReportBuilder BuildSummary(string stats);
    public Report GetReport();
}
