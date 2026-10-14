namespace Acme.Workforce.Absence.Tests.Accrual.Slot;

using Acme.Workforce.Absence.Accrual.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class AccrualForecastViewChecks
{
    public static void AccrualForecastViewShowsTheStatusInCapitals()
    {
        var item = AccrualForecast.NewAccrualForecast("code-617", "name-440");
        var view = AccrualForecastView.ToAccrualForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AccrualForecastView.ToAccrualForecastViews(new List<AccrualForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AccrualForecastViewShowsTheStatusInCapitals();
    }
}
