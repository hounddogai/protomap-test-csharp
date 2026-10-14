namespace Acme.Workforce.Workplace.Tests.Occupancy.Score;

using Acme.Workforce.Workplace.Occupancy.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class OccupancyAdjustmentViewChecks
{
    public static void OccupancyAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = OccupancyAdjustment.NewOccupancyAdjustment("code-363", "title-454");
        var view = OccupancyAdjustmentView.ToOccupancyAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(OccupancyAdjustmentView.ToOccupancyAdjustmentViews(new List<OccupancyAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        OccupancyAdjustmentViewShowsTheStatusInCapitals();
    }
}
