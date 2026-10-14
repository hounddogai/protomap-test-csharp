namespace Acme.Workforce.Engagement.Tests.TownHall.Target;

using Acme.Workforce.Engagement.TownHall.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class BatchedTownHallBatchChecks
{
    public static void BatchedTownHallBatchStartsInIdle()
    {
        var item = BatchedTownHallBatch.NewBatchedTownHallBatch("code-201", "label-534");
        Check.Equal(item.Status, BatchedTownHallBatchStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedTownHallBatchMovesToStopped()
    {
        var item = BatchedTownHallBatch.NewBatchedTownHallBatch("code-201", "label-534");
        try
        {
            item.MoveTo(BatchedTownHallBatchStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedTownHallBatchStatus.Stopped, "status");
    }

    public static void BatchedTownHallBatchRejectsMovesFromStopped()
    {
        var item = BatchedTownHallBatch.NewBatchedTownHallBatch("code-201", "label-534");
        item.Status = BatchedTownHallBatchStatus.Stopped;
        Check.Fails(() => item.MoveTo(BatchedTownHallBatchStatus.Idle), "move");
    }

    public static void RunAll()
    {
        BatchedTownHallBatchStartsInIdle();
        BatchedTownHallBatchMovesToStopped();
        BatchedTownHallBatchRejectsMovesFromStopped();
    }
}
