using BudzetDomowy.Models;

namespace BudzetDomowy.Core.Patterns.StrategyMethod;

public interface IForecastingStrategy
{
    public decimal PredictNextMonth(List<Transaction> history);
}
