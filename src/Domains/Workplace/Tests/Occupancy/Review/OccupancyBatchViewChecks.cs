namespace Acme.Workforce.Workplace.Tests.Occupancy.Review;

using Acme.Workforce.Workplace.Occupancy.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class OccupancyBatchViewChecks
{
    public static void OccupancyBatchViewShowsTheStatusInCapitals()
    {
        var item = OccupancyBatch.NewOccupancyBatch("code-264", "label-146");
        var view = OccupancyBatchView.ToOccupancyBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(OccupancyBatchView.ToOccupancyBatchViews(new List<OccupancyBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        OccupancyBatchViewShowsTheStatusInCapitals();
    }
}
