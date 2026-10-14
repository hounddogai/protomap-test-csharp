namespace Acme.Workforce.Workplace.Tests.Occupancy.Estimate;

using Acme.Workforce.Workplace.Occupancy.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class OccupancyTierViewChecks
{
    public static void OccupancyTierViewShowsTheStatusInCapitals()
    {
        var item = OccupancyTier.NewOccupancyTier("code-633", "title-778");
        var view = OccupancyTierView.ToOccupancyTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(OccupancyTierView.ToOccupancyTierViews(new List<OccupancyTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        OccupancyTierViewShowsTheStatusInCapitals();
    }
}
