namespace Acme.Workforce.Talent.Tests.Mobility.Forecast;

using Acme.Workforce.Talent.Mobility.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class MobilityForecastViewChecks
{
    public static void MobilityForecastViewShowsTheStatusInCapitals()
    {
        var item = MobilityForecast.NewMobilityForecast("code-269", "name-404");
        var view = MobilityForecastView.ToMobilityForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MobilityForecastView.ToMobilityForecastViews(new List<MobilityForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MobilityForecastViewShowsTheStatusInCapitals();
    }
}
