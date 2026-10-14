namespace Acme.Workforce.Engagement.Tests.TownHall.Review;

using Acme.Workforce.Engagement.TownHall.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class TownHallForecastViewChecks
{
    public static void TownHallForecastViewShowsTheStatusInCapitals()
    {
        var item = TownHallForecast.NewTownHallForecast("code-205", "title-274");
        var view = TownHallForecastView.ToTownHallForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TownHallForecastView.ToTownHallForecastViews(new List<TownHallForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TownHallForecastViewShowsTheStatusInCapitals();
    }
}
