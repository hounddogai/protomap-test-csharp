namespace Acme.Workforce.Workplace.Tests.Occupancy.Rule;

using Acme.Workforce.Workplace.Occupancy.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class BatchedOccupancySnapshotChecks
{
    public static void BatchedOccupancySnapshotStartsInOpen()
    {
        var item = BatchedOccupancySnapshot.NewBatchedOccupancySnapshot("code-118", "title-758");
        Check.Equal(item.Status, BatchedOccupancySnapshotStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedOccupancySnapshotMovesToInReview()
    {
        var item = BatchedOccupancySnapshot.NewBatchedOccupancySnapshot("code-118", "title-758");
        try
        {
            item.MoveTo(BatchedOccupancySnapshotStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedOccupancySnapshotStatus.InReview, "status");
    }

    public static void BatchedOccupancySnapshotRejectsMovesFromArchived()
    {
        var item = BatchedOccupancySnapshot.NewBatchedOccupancySnapshot("code-118", "title-758");
        item.Status = BatchedOccupancySnapshotStatus.Archived;
        Check.Fails(() => item.MoveTo(BatchedOccupancySnapshotStatus.Open), "move");
    }

    public static void RunAll()
    {
        BatchedOccupancySnapshotStartsInOpen();
        BatchedOccupancySnapshotMovesToInReview();
        BatchedOccupancySnapshotRejectsMovesFromArchived();
    }
}
