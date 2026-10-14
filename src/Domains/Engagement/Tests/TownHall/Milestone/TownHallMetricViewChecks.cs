namespace Acme.Workforce.Engagement.Tests.TownHall.Milestone;

using Acme.Workforce.Engagement.TownHall.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class TownHallMetricViewChecks
{
    public static void TownHallMetricViewShowsTheStatusInCapitals()
    {
        var item = TownHallMetric.NewTownHallMetric("code-128", "title-815");
        var view = TownHallMetricView.ToTownHallMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TownHallMetricView.ToTownHallMetricViews(new List<TownHallMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TownHallMetricViewShowsTheStatusInCapitals();
    }
}
