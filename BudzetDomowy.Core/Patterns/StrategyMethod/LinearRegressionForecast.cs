using BudzetDomowy.Core.Models;

namespace BudzetDomowy.Core.Patterns.StrategyMethod;

public class LinearRegressionForecast : IForecastingStrategy
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
            .OrderBy(g => g.Key.Year)
            .ThenBy(g => g.Key.Month)
            .Select(g => g.Sum(t => t.Amount))
            .ToList();

        if (monthlyTotals.Count < 2)
        {
            throw new InvalidOperationException("Do regresji liniowej wymagane są co najmniej dwa miesiące danych.");
        }

        int n = monthlyTotals.Count;

        // x = 1..n
        double sumX = n * (n + 1) / 2.0;
        double sumX2 = n * (n + 1) * (2 * n + 1) / 6.0;

        double sumY = monthlyTotals.Sum(v => (double)v);
        double sumXY = monthlyTotals
            .Select((v, i) => (i + 1) * (double)v)
            .Sum();

        double denominator = n * sumX2 - sumX * sumX;
        if (denominator == 0)
        {
            throw new InvalidOperationException("Nie można obliczyć regresji liniowej (dzielenie przez zero).");
        }

        double slope = (n * sumXY - sumX * sumY) / denominator;
        double intercept = (sumY - slope * sumX) / n;

        double prediction = slope * (n + 1) + intercept;

        return Math.Round((decimal)prediction, 2);
    }
}
