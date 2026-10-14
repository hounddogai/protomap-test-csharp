namespace Acme.Workforce.Engagement.Tests.TownHall.Target;

using Acme.Workforce.Engagement.TownHall.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class BatchedTownHallBatchViewChecks
{
    public static void BatchedTownHallBatchViewShowsTheStatusInCapitals()
    {
        var item = BatchedTownHallBatch.NewBatchedTownHallBatch("code-201", "label-534");
        var view = BatchedTownHallBatchView.ToBatchedTownHallBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedTownHallBatchView.ToBatchedTownHallBatchViews(new List<BatchedTownHallBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedTownHallBatchViewShowsTheStatusInCapitals();
    }
}
