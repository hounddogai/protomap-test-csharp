namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyAdjustment;

using Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

/// <summary>Checks of occupancy adjustment code, run by the project's runner.</summary>
public static class RollingOccupancyAdjustmentViewChecks
{
    public static void RollingOccupancyAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = RollingOccupancyAdjustment.NewRollingOccupancyAdjustment("code-638", "label-392");
        var view = RollingOccupancyAdjustmentView.ToRollingOccupancyAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingOccupancyAdjustmentView.ToRollingOccupancyAdjustmentViews(new List<RollingOccupancyAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingOccupancyAdjustmentViewShowsTheStatusInCapitals();
    }
}
