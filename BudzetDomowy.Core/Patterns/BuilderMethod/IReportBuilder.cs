using BudzetDomowy.Core.Models;
using System;

namespace BudzetDomowy.Core.Patterns.BuilderMethod;

// Interfejs Budowniczego.
// Definiuje kroki niezbędne do stworzenia raportu, niezależnie od jego formatu (PDF/CSV).
public interface IReportBuilder
{
    IReportBuilder BuildHeader();
    IReportBuilder BuildTable(List<Transaction> transactions);
    IReportBuilder BuildFooter(string footer);
    IReportBuilder BuildSummary(string stats);
    Report GetReport();
}