using BudzetDomowy.Core.Models;

namespace BudzetDomowy.Core.Patterns.StrategyMethod;

// Interfejs Strategii.
// Umożliwia dynamiczną wymianę algorytmów prognozowania bez zmiany kodu BudgetManagera.
public interface IForecastingStrategy
{
    public decimal PredictNextMonth(List<Transaction> history);
}
