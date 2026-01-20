namespace BudzetDomowy.Core.Patterns.ObserverMethod
{
    // Interfejs Obserwatora.
    // Umożliwia luźne powiązanie (decoupling) - BudgetManager nie musi wiedzieć,
    // kto i w jaki sposób reaguje na zmiany w budżecie.
    public interface IBudgetObserver
    {
        void Update(double balance, double limit);
    }
}