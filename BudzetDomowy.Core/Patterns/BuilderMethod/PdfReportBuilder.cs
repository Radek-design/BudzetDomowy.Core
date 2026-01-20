using System.Collections.Generic;
using System.IO;
using BudzetDomowy.Core.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BudzetDomowy.Core.Patterns.BuilderMethod;

// Konkretny Budowniczy dla formatu PDF.
// Wykorzystuje bibliotekę QuestPDF do renderowania graficznego raportu.
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
        if (transactions != null)
            _transactions = transactions;
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
        // Generujemy unikalną nazwę pliku
        string fileName = $"Raport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

        // Tworzymy dokument PDF
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12));

                // Nagłówek
                page.Header()
                    .Text(_header)
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                // Treść (Tabela)
                page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                {
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(100); // Data
                            columns.RelativeColumn();    // Opis
                            columns.RelativeColumn();    // Kategoria
                            columns.ConstantColumn(80);  // Kwota
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Data");
                            header.Cell().Element(CellStyle).Text("Opis");
                            header.Cell().Element(CellStyle).Text("Kategoria");
                            header.Cell().Element(CellStyle).Text("Kwota").AlignRight();
                        });

                        foreach (var t in _transactions)
                        {
                            table.Cell().Element(CellStyle).Text(t.Date.ToString("yyyy-MM-dd"));
                            table.Cell().Element(CellStyle).Text(t.Description);
                            table.Cell().Element(CellStyle).Text(t.Category); // Jeśli masz pole Category

                            var color = t is Expense ? Colors.Red.Medium : Colors.Green.Medium;
                            table.Cell().Element(CellStyle).Text($"{t.Amount:F2}").FontColor(color).AlignRight();
                        }

                        static IContainer CellStyle(IContainer container) =>
                            container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                    });

                    col.Item().PaddingTop(20).Text("PODSUMOWANIE:").Bold();
                    col.Item().Text(_summary);
                });

                // Stopka
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span(_footer);
                    x.Span(" | Strona ");
                    x.CurrentPageNumber();
                });
            });
        })
        .GeneratePdf(fileName); // Zapis fizyczny na dysku

        // Zwracamy informację o sukcesie
        return new Report(_header, _footer, $"WYGENEROWANO PLIK PDF:\n{Path.GetFullPath(fileName)}");
    }
}