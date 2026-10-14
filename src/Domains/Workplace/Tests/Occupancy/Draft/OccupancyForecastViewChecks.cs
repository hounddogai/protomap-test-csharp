namespace Acme.Workforce.Workplace.Tests.Occupancy.Draft;

using Acme.Workforce.Workplace.Occupancy.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class OccupancyForecastViewChecks
{
    public static void OccupancyForecastViewShowsTheStatusInCapitals()
    {
        var item = OccupancyForecast.NewOccupancyForecast("code-24", "title-518");
        var view = OccupancyForecastView.ToOccupancyForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(OccupancyForecastView.ToOccupancyForecastViews(new List<OccupancyForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        OccupancyForecastViewShowsTheStatusInCapitals();
    }
}
