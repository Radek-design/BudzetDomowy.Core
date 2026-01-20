using BudzetDomowy.Core.Models;

namespace BudzetDomowy.Core.Patterns.StrategyMethod;

// Interfejs Strategii prognozowania.
// Umożliwia dynamiczną wymianę algorytmów obliczeniowych w trakcie działania programu (Runtime).
public interface IForecastingStrategy
{
    decimal PredictNextMonth(List<Transaction> history);
}