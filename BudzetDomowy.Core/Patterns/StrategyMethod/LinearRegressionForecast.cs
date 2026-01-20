using BudzetDomowy.Core.Models;

namespace BudzetDomowy.Core.Patterns.StrategyMethod;

// Strategia Zaawansowana: Wykorzystuje regresję liniową (Metoda Najmniejszych Kwadratów) do wykrycia trendu.
public class LinearRegressionForecast : IForecastingStrategy
{
    public decimal PredictNextMonth(List<Transaction> history)
    {
        if (history == null) throw new ArgumentNullException(nameof(history));
        if (history.Count == 0) throw new InvalidOperationException("Brak danych.");

        // Przygotowanie danych (X: numer miesiąca, Y: suma kwot)
        var monthlyTotals = history
            .GroupBy(t => new { t.Date.Year, t.Date.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => g.Sum(t => t.Amount))
            .ToList();

        if (monthlyTotals.Count < 2) throw new InvalidOperationException("Wymagane min. 2 miesiące danych.");

        int n = monthlyTotals.Count;
        double sumX = n * (n + 1) / 2.0;
        double sumX2 = n * (n + 1) * (2 * n + 1) / 6.0;
        double sumY = monthlyTotals.Sum(v => (double)v);
        double sumXY = monthlyTotals.Select((v, i) => (i + 1) * (double)v).Sum();

        double denominator = n * sumX2 - sumX * sumX;
        if (denominator == 0) throw new InvalidOperationException("Błąd obliczeń regresji.");

        double slope = (n * sumXY - sumX * sumY) / denominator;
        double intercept = (sumY - slope * sumX) / n;

        // Prognoza dla x = n + 1
        double prediction = slope * (n + 1) + intercept;

        return Math.Round((decimal)prediction, 2);
    }
}