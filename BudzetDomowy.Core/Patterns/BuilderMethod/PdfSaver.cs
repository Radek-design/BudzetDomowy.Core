using BudzetDomowy.Core.Models;
using QuestPDF.Fluent;

namespace BudzetDomowy.Core.Patterns.BuilderMethod;

public class PdfSaver
{
    public static void SaveToPdfFile(Report report, string path)
    {
        if (report == null)
            throw new ArgumentNullException(nameof(report), "Raport nie może być null.");

        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentNullException(nameof(path), "Ścieżka do pliku PDF nie może być pusta.");

        Document.Create(container =>
            {
                container.Page(page =>
                {
                    // Optional: Size, margins, styling
                    page.Margin(20);

                    // Header
                    page.Header()
                        .Text(report.Header);

                    // Content
                    page.Content()
                        .Text(report.Content);

                    // Footer
                    page.Footer()
                        .Text(report.Footer);
                });
            })
            .GeneratePdf(path);
    }
}
