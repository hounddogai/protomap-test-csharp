namespace Acme.Workforce.Workplace.Tests.Occupancy.Batch;

using Acme.Workforce.Workplace.Occupancy.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class WeightedOccupancyBatchViewChecks
{
    public static void WeightedOccupancyBatchViewShowsTheStatusInCapitals()
    {
        var item = WeightedOccupancyBatch.NewWeightedOccupancyBatch("code-566", "name-777");
        var view = WeightedOccupancyBatchView.ToWeightedOccupancyBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedOccupancyBatchView.ToWeightedOccupancyBatchViews(new List<WeightedOccupancyBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedOccupancyBatchViewShowsTheStatusInCapitals();
    }
}
