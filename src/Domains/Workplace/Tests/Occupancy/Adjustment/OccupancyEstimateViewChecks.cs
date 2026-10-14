namespace Acme.Workforce.Workplace.Tests.Occupancy.Adjustment;

using Acme.Workforce.Workplace.Occupancy.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class OccupancyEstimateViewChecks
{
    public static void OccupancyEstimateViewShowsTheStatusInCapitals()
    {
        var item = OccupancyEstimate.NewOccupancyEstimate("code-105", "title-460");
        var view = OccupancyEstimateView.ToOccupancyEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(OccupancyEstimateView.ToOccupancyEstimateViews(new List<OccupancyEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        OccupancyEstimateViewShowsTheStatusInCapitals();
    }
}
