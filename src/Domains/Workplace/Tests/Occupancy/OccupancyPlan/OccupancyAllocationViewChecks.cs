namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyPlan;

using Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

/// <summary>Checks of occupancy plan code, run by the project's runner.</summary>
public static class OccupancyAllocationViewChecks
{
    public static void OccupancyAllocationViewShowsTheStatusInCapitals()
    {
        var item = OccupancyAllocation.NewOccupancyAllocation("code-721", "label-409");
        var view = OccupancyAllocationView.ToOccupancyAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(OccupancyAllocationView.ToOccupancyAllocationViews(new List<OccupancyAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        OccupancyAllocationViewShowsTheStatusInCapitals();
    }
}
