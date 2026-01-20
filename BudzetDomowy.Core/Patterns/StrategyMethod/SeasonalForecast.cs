using BudzetDomowy.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BudzetDomowy.Core.Patterns.StrategyMethod;

// Strategia Sezonowa.
// Przewiduje wydatki na podstawie tego samego miesiąca w poprzednich latach.
// Przydatna do wykrywania cyklicznych wzrostów wydatków (np. Święta, wakacje).
public class SeasonalForecast : IForecastingStrategy
{
    public decimal PredictNextMonth(List<Transaction> history)
    {
        if (history == null)
            throw new ArgumentNullException(nameof(history));

        if (!history.Any())
            throw new InvalidOperationException("Brak historii transakcji.");

        // Ustalamy, jaki miesiąc będziemy prognozować (następny po obecnym)
        var nextMonth = DateTime.Now.AddMonths(1).Month;

        // Filtrujemy dane tylko z tego konkretnego miesiąca na przestrzeni lat
        var seasonalTotals = history
            .Where(t => t.Date.Month == nextMonth)
            .GroupBy(t => t.Date.Year)
            .Select(g => g.Sum(t => t.Amount))
            .ToList();

        // Wymagamy danych z co najmniej dwóch lat, aby mówić o "sezonowości"
        if (seasonalTotals.Count < 2)
            throw new InvalidOperationException(
                "Do prognozy sezonowej wymagane są dane z co najmniej dwóch lat dla tego miesiąca.");

        return Math.Round((decimal)seasonalTotals.Average(), 2);
    }
}