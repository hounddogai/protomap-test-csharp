namespace Acme.Workforce.Workplace.Tests.Occupancy.Target;

using Acme.Workforce.Workplace.Occupancy.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class OccupancyTargetViewChecks
{
    public static void OccupancyTargetViewShowsTheStatusInCapitals()
    {
        var item = OccupancyTarget.NewOccupancyTarget("code-990", "name-997");
        var view = OccupancyTargetView.ToOccupancyTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(OccupancyTargetView.ToOccupancyTargetViews(new List<OccupancyTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        OccupancyTargetViewShowsTheStatusInCapitals();
    }
}
