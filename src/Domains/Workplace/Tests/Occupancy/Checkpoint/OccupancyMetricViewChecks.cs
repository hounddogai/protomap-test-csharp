namespace Acme.Workforce.Workplace.Tests.Occupancy.Checkpoint;

using Acme.Workforce.Workplace.Occupancy.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class OccupancyMetricViewChecks
{
    public static void OccupancyMetricViewShowsTheStatusInCapitals()
    {
        var item = OccupancyMetric.NewOccupancyMetric("code-361", "name-481");
        var view = OccupancyMetricView.ToOccupancyMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(OccupancyMetricView.ToOccupancyMetricViews(new List<OccupancyMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        OccupancyMetricViewShowsTheStatusInCapitals();
    }
}
