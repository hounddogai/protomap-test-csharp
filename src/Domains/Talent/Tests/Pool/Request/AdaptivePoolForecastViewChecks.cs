namespace Acme.Workforce.Talent.Tests.Pool.Request;

using Acme.Workforce.Talent.Pool.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class AdaptivePoolForecastViewChecks
{
    public static void AdaptivePoolForecastViewShowsTheStatusInCapitals()
    {
        var item = AdaptivePoolForecast.NewAdaptivePoolForecast("code-383", "title-779");
        var view = AdaptivePoolForecastView.ToAdaptivePoolForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptivePoolForecastView.ToAdaptivePoolForecastViews(new List<AdaptivePoolForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptivePoolForecastViewShowsTheStatusInCapitals();
    }
}
