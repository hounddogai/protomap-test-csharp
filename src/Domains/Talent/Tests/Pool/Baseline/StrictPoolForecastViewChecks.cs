namespace Acme.Workforce.Talent.Tests.Pool.Baseline;

using Acme.Workforce.Talent.Pool.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class StrictPoolForecastViewChecks
{
    public static void StrictPoolForecastViewShowsTheStatusInCapitals()
    {
        var item = StrictPoolForecast.NewStrictPoolForecast("code-282", "label-743");
        var view = StrictPoolForecastView.ToStrictPoolForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictPoolForecastView.ToStrictPoolForecastViews(new List<StrictPoolForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictPoolForecastViewShowsTheStatusInCapitals();
    }
}
