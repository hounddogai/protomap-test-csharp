namespace Acme.Workforce.Workplace.Tests.Occupancy.Batch;

using Acme.Workforce.Workplace.Occupancy.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class WeightedOccupancyBatchChecks
{
    public static void WeightedOccupancyBatchStartsInPlanned()
    {
        var item = WeightedOccupancyBatch.NewWeightedOccupancyBatch("code-566", "name-777");
        Check.Equal(item.Status, WeightedOccupancyBatchStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedOccupancyBatchMovesToInProgress()
    {
        var item = WeightedOccupancyBatch.NewWeightedOccupancyBatch("code-566", "name-777");
        try
        {
            item.MoveTo(WeightedOccupancyBatchStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedOccupancyBatchStatus.InProgress, "status");
    }

    public static void WeightedOccupancyBatchRejectsMovesFromCancelled()
    {
        var item = WeightedOccupancyBatch.NewWeightedOccupancyBatch("code-566", "name-777");
        item.Status = WeightedOccupancyBatchStatus.Cancelled;
        Check.Fails(() => item.MoveTo(WeightedOccupancyBatchStatus.Planned), "move");
    }

    public static void RunAll()
    {
        WeightedOccupancyBatchStartsInPlanned();
        WeightedOccupancyBatchMovesToInProgress();
        WeightedOccupancyBatchRejectsMovesFromCancelled();
    }
}
