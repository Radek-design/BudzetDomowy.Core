namespace BudzetDomowy.Core.Patterns.ObserverMethod
{
    // Interfejs Obserwatora.
    // Pozwala na luźne powiązanie (decoupling) między BudgetManagerem a systemami powiadomień.
    public interface IBudgetObserver
    {
        void Update(double balance, double limit);
    }
}