namespace Acme.Workforce.Workplace.Tests.Occupancy.Draft;

using Acme.Workforce.Workplace.Occupancy.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class GlobalOccupancyForecastViewChecks
{
    public static void GlobalOccupancyForecastViewShowsTheStatusInCapitals()
    {
        var item = GlobalOccupancyForecast.NewGlobalOccupancyForecast("code-378", "label-28");
        var view = GlobalOccupancyForecastView.ToGlobalOccupancyForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalOccupancyForecastView.ToGlobalOccupancyForecastViews(new List<GlobalOccupancyForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalOccupancyForecastViewShowsTheStatusInCapitals();
    }
}
