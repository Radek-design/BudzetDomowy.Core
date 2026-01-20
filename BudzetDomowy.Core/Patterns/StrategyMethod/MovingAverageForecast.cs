using BudzetDomowy.Core.Models;

namespace BudzetDomowy.Core.Patterns.StrategyMethod;

public class MovingAverageForecast : IForecastingStrategy
{
    private readonly int _months;

    public MovingAverageForecast(int months = 3)
    {
        if (months <= 0)
            throw new ArgumentOutOfRangeException(nameof(months));

        _months = months;
    }

    public decimal PredictNextMonth(List<Transaction> history)
    {
        if (history == null)
            throw new ArgumentNullException(nameof(history));

        var monthlyTotals = history
            .GroupBy(t => new { t.Date.Year, t.Date.Month })
            .OrderBy(g => g.Key.Year)
            .ThenBy(g => g.Key.Month)
            .Select(g => g.Sum(t => t.Amount))
            .ToList();

        if (monthlyTotals.Count < _months)
            throw new InvalidOperationException(
                $"Wymagane co najmniej {_months} miesiące danych.");

        var average = monthlyTotals
            .TakeLast(_months)
            .Average();

        return Math.Round((decimal)average, 2);
    }
}
