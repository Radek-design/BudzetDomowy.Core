namespace BudzetDomowy.Core.Patterns.ObserverMethod
{
    public interface IBudgetObserver
    {
        void Update(double balance, double limit);
    }
}