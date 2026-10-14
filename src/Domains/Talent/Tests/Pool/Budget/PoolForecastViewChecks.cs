namespace Acme.Workforce.Talent.Tests.Pool.Budget;

using Acme.Workforce.Talent.Pool.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class PoolForecastViewChecks
{
    public static void PoolForecastViewShowsTheStatusInCapitals()
    {
        var item = PoolForecast.NewPoolForecast("code-8", "name-660");
        var view = PoolForecastView.ToPoolForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PoolForecastView.ToPoolForecastViews(new List<PoolForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PoolForecastViewShowsTheStatusInCapitals();
    }
}
