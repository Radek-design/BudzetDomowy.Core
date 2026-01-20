using BudzetDomowy.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BudzetDomowy.Core.Patterns.StrategyMethod;

// Strategia Średniej Ruchomej (Moving Average).
// Oblicza prognozę na podstawie średniej z ustalonej liczby ostatnich miesięcy (np. ostatnich 3).
// Pozwala wygładzić krótkoterminowe wahania.
public class MovingAverageForecast : IForecastingStrategy
{
    private readonly int _months;

    // Inicjalizuje strategię z określonym oknem czasowym.
    // <param name="months">Liczba miesięcy wstecz do uwzględnienia w średniej (domyślnie 3).</param>
    public MovingAverageForecast(int months = 3)
    {
        if (months <= 0)
            throw new ArgumentOutOfRangeException(nameof(months), "Liczba miesięcy musi być większa od zera.");

        _months = months;
    }

    public decimal PredictNextMonth(List<Transaction> history)
    {
        if (history == null)
            throw new ArgumentNullException(nameof(history));

        // Grupujemy i sumujemy wydatki miesięczne
        var monthlyTotals = history
            .GroupBy(t => new { t.Date.Year, t.Date.Month })
            .OrderBy(g => g.Key.Year)
            .ThenBy(g => g.Key.Month)
            .Select(g => g.Sum(t => t.Amount))
            .ToList();

        // Sprawdzamy, czy mamy wystarczającą historię
        if (monthlyTotals.Count < _months)
            throw new InvalidOperationException(
                $"Wymagane co najmniej {_months} miesiące danych historycznych do obliczenia średniej ruchomej.");

        // Pobieramy ostatnie N miesięcy i liczymy średnią
        var average = monthlyTotals
            .TakeLast(_months)
            .Average();

        return Math.Round((decimal)average, 2);
    }
}