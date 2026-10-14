namespace Acme.Workforce.Engagement.Tests.TownHall.BatchedHallRule;

using Acme.Workforce.Engagement.TownHall.BatchedHallRule;

/// <summary>Checks of batched hall rule code, run by the project's runner.</summary>
public static class SeasonalTownHallMetricViewChecks
{
    public static void SeasonalTownHallMetricViewShowsTheStatusInCapitals()
    {
        var item = SeasonalTownHallMetric.NewSeasonalTownHallMetric("code-396", "name-505");
        var view = SeasonalTownHallMetricView.ToSeasonalTownHallMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalTownHallMetricView.ToSeasonalTownHallMetricViews(new List<SeasonalTownHallMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalTownHallMetricViewShowsTheStatusInCapitals();
    }
}
