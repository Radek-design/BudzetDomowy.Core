using BudzetDomowy.Core.Models;
using System;

namespace BudzetDomowy.Core.Patterns.StrategyMethod;

// Strategia Naiwna: Zakłada, że wydatki w przyszłym miesiącu będą identyczne jak w ostatnim zamkniętym miesiącu.
public class LastMonthForecast : IForecastingStrategy
{
    public decimal PredictNextMonth(List<Transaction> history)
    {
        if (history == null) throw new ArgumentNullException(nameof(history));
        if (!history.Any()) throw new InvalidOperationException("Brak historii transakcji.");

        // Pobieramy ostatni miesiąc chronologicznie
        var lastMonthTotal = history
            .GroupBy(t => new { t.Date.Year, t.Date.Month })
            .OrderBy(g => g.Key.Year)
            .ThenBy(g => g.Key.Month)
            .Last()
            .Sum(t => t.Amount);

        return Math.Round((decimal)lastMonthTotal, 2);
    }
}