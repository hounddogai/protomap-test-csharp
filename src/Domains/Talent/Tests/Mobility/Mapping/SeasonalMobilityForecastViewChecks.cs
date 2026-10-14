namespace Acme.Workforce.Talent.Tests.Mobility.Mapping;

using Acme.Workforce.Talent.Mobility.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class SeasonalMobilityForecastViewChecks
{
    public static void SeasonalMobilityForecastViewShowsTheStatusInCapitals()
    {
        var item = SeasonalMobilityForecast.NewSeasonalMobilityForecast("code-639", "label-248");
        var view = SeasonalMobilityForecastView.ToSeasonalMobilityForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalMobilityForecastView.ToSeasonalMobilityForecastViews(new List<SeasonalMobilityForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalMobilityForecastViewShowsTheStatusInCapitals();
    }
}
