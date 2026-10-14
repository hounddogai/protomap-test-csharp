namespace Acme.Workforce.Talent.Tests.CareerPath.PathRule;

using Acme.Workforce.Talent.CareerPath.PathRule;

/// <summary>Checks of path rule code, run by the project's runner.</summary>
public static class TieredCareerPathForecastViewChecks
{
    public static void TieredCareerPathForecastViewShowsTheStatusInCapitals()
    {
        var item = TieredCareerPathForecast.NewTieredCareerPathForecast("code-777", "name-110");
        var view = TieredCareerPathForecastView.ToTieredCareerPathForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredCareerPathForecastView.ToTieredCareerPathForecastViews(new List<TieredCareerPathForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredCareerPathForecastViewShowsTheStatusInCapitals();
    }
}
