namespace Acme.Workforce.Workplace.Tests.Occupancy.Estimate;

using Acme.Workforce.Workplace.Occupancy.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class TieredOccupancyAllocationViewChecks
{
    public static void TieredOccupancyAllocationViewShowsTheStatusInCapitals()
    {
        var item = TieredOccupancyAllocation.NewTieredOccupancyAllocation("code-220", "name-985");
        var view = TieredOccupancyAllocationView.ToTieredOccupancyAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredOccupancyAllocationView.ToTieredOccupancyAllocationViews(new List<TieredOccupancyAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredOccupancyAllocationViewShowsTheStatusInCapitals();
    }
}
