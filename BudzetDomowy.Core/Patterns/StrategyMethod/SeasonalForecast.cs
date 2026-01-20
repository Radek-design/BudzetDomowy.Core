using BudzetDomowy.Core.Models;

namespace BudzetDomowy.Core.Patterns.StrategyMethod;

public class SeasonalForecast : IForecastingStrategy
{
    public decimal PredictNextMonth(List<Transaction> history)
    {
        if (history == null)
            throw new ArgumentNullException(nameof(history));

        if (!history.Any())
            throw new InvalidOperationException("Brak historii transakcji.");

        var nextMonth = DateTime.Now.AddMonths(1).Month;

        var seasonalTotals = history
            .Where(t => t.Date.Month == nextMonth)
            .GroupBy(t => t.Date.Year)
            .Select(g => g.Sum(t => t.Amount))
            .ToList();

        if (seasonalTotals.Count < 2)
            throw new InvalidOperationException(
                "Do prognozy sezonowej wymagane są dane z co najmniej dwóch lat.");

        return Math.Round((decimal)seasonalTotals.Average(), 2);
    }
}
