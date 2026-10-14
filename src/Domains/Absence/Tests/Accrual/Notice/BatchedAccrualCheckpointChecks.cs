namespace Acme.Workforce.Absence.Tests.Accrual.Notice;

using Acme.Workforce.Absence.Accrual.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class BatchedAccrualCheckpointChecks
{
    public static void BatchedAccrualCheckpointStartsInIdle()
    {
        var item = BatchedAccrualCheckpoint.NewBatchedAccrualCheckpoint("code-107", "label-375");
        Check.Equal(item.Status, BatchedAccrualCheckpointStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedAccrualCheckpointMovesToStopped()
    {
        var item = BatchedAccrualCheckpoint.NewBatchedAccrualCheckpoint("code-107", "label-375");
        try
        {
            item.MoveTo(BatchedAccrualCheckpointStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedAccrualCheckpointStatus.Stopped, "status");
    }

    public static void BatchedAccrualCheckpointRejectsMovesFromStopped()
    {
        var item = BatchedAccrualCheckpoint.NewBatchedAccrualCheckpoint("code-107", "label-375");
        item.Status = BatchedAccrualCheckpointStatus.Stopped;
        Check.Fails(() => item.MoveTo(BatchedAccrualCheckpointStatus.Idle), "move");
    }

    public static void RunAll()
    {
        BatchedAccrualCheckpointStartsInIdle();
        BatchedAccrualCheckpointMovesToStopped();
        BatchedAccrualCheckpointRejectsMovesFromStopped();
    }
}
