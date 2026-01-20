using System.Collections.Generic;
using System.IO;
using BudzetDomowy.Core.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BudzetDomowy.Core.Patterns.BuilderMethod;

// Budowniczy raportów PDF.
// Wykorzystuje bibliotekę QuestPDF do generowania sformatowanego dokumentu z tabelami i kolorami.
public class PdfReportBuilder : IReportBuilder
{
    private List<Transaction> _transactions = new();
    private string _summary = string.Empty;
    private string _header = string.Empty;
    private string _footer = string.Empty;

    public IReportBuilder BuildHeader()
    {
        _header = "Raport Finansowy";
        return this;
    }

    public IReportBuilder BuildTable(List<Transaction> transactions)
    {
        if (transactions != null) _transactions = transactions;
        return this;
    }

    public IReportBuilder BuildSummary(string stats)
    {
        _summary = stats;
        return this;
    }

    public IReportBuilder BuildFooter(string footer)
    {
        _footer = footer;
        return this;
    }

    public Report GetReport()
    {
        string fileName = $"Raport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

        // Konfiguracja dokumentu QuestPDF
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(12));

                page.Header().Text(_header).SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c => {
                            c.ConstantColumn(100);
                            c.RelativeColumn();
                            c.RelativeColumn();
                            c.ConstantColumn(80);
                        });

                        table.Header(h => {
                            h.Cell().Text("Data").Bold();
                            h.Cell().Text("Opis").Bold();
                            h.Cell().Text("Kategoria").Bold();
                            h.Cell().Text("Kwota").Bold().AlignRight();
                        });

                        foreach (var t in _transactions)
                        {
                            table.Cell().Text(t.Date.ToString("yyyy-MM-dd"));
                            table.Cell().Text(t.Description);
                            table.Cell().Text(t.Category);
                            var color = t is Expense ? Colors.Red.Medium : Colors.Green.Medium;
                            table.Cell().Text($"{t.Amount:F2}").FontColor(color).AlignRight();
                        }
                    });

                    col.Item().PaddingTop(20).Text($"PODSUMOWANIE: {_summary}").Bold();
                });

                page.Footer().AlignCenter().Text(x => {
                    x.Span(_footer);
                    x.Span(" | Strona ");
                    x.CurrentPageNumber();
                });
            });
        }).GeneratePdf(fileName);

        return new Report(_header, _footer, $"WYGENEROWANO PLIK PDF:\n{Path.GetFullPath(fileName)}");
    }
}