namespace Acme.Workforce.Talent.Tests.Pool.Window;

using Acme.Workforce.Talent.Pool.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class DefaultPoolForecastViewChecks
{
    public static void DefaultPoolForecastViewShowsTheStatusInCapitals()
    {
        var item = DefaultPoolForecast.NewDefaultPoolForecast("code-50", "label-207");
        var view = DefaultPoolForecastView.ToDefaultPoolForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultPoolForecastView.ToDefaultPoolForecastViews(new List<DefaultPoolForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultPoolForecastViewShowsTheStatusInCapitals();
    }
}
