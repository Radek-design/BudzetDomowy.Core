using BudzetDomowy.Core.Models;

namespace BudzetDomowy.Core.Patterns.StrategyMethod;

public interface IForecastingStrategy
{
    public void PredictNextMonth(List<Transaction> history);
}
