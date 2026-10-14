namespace Acme.Workforce.Absence.Tests.Balance.Target;

using Acme.Workforce.Absence.Balance.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class BalanceForecastViewChecks
{
    public static void BalanceForecastViewShowsTheStatusInCapitals()
    {
        var item = BalanceForecast.NewBalanceForecast("code-218", "name-321");
        var view = BalanceForecastView.ToBalanceForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BalanceForecastView.ToBalanceForecastViews(new List<BalanceForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BalanceForecastViewShowsTheStatusInCapitals();
    }
}
