using BudzetDomowy.Core.Models;

namespace BudzetDomowy.Core.Patterns.StrategyMethod;

public class AverageForecast : IForecastingStrategy
{
    public decimal PredictNextMonth(List<Transaction> history)
    {
        if (history == null)
        {
            throw new ArgumentNullException(nameof(history), "Historia transakcji nie może być null.");
        }

        if (history.Count == 0)
        {
            throw new InvalidOperationException("Brak historii transakcji do wykonania prognozy.");
        }

        var monthlyTotals = history
            .GroupBy(t => new { t.Date.Year, t.Date.Month })
            .Select(g => g.Sum(t => t.Amount))
            .ToList();

        if (monthlyTotals.Count == 0)
        {
            throw new InvalidOperationException("Niewystarczające dane do wykonania prognozy.");
        }

        var averageForecast = monthlyTotals.Average();

        return Math.Round((decimal)averageForecast, 2);
    }
}
