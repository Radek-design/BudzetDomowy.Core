using BudzetDomowy.Core.Models;

namespace BudzetDomowy.Core.Patterns.StrategyMethod;

// Strategia naiwna: Zakłada, że przyszły miesiąc będzie taki sam jak ostatni.
public class LastMonthForecast : IForecastingStrategy
{
    public decimal PredictNextMonth(List<Transaction> history)
    {
        if (history == null)
            throw new ArgumentNullException(nameof(history));

        if (!history.Any())
            throw new InvalidOperationException("Brak historii transakcji.");

        // Sortujemy chronologicznie i bierzemy ostatnią grupę (miesiąc)
        var lastMonthTotal = history
            .GroupBy(t => new { t.Date.Year, t.Date.Month })
            .OrderBy(g => g.Key.Year)
            .ThenBy(g => g.Key.Month)
            .Last()
            .Sum(t => t.Amount);

        return Math.Round((decimal)lastMonthTotal, 2);
    }
}
