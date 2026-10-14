namespace Acme.Workforce.Orgdesign.Tests.Reorg.Checkpoint;

using Acme.Workforce.Orgdesign.Reorg.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class BatchedReorgCheckpointChecks
{
    public static void BatchedReorgCheckpointStartsInRequested()
    {
        var item = BatchedReorgCheckpoint.NewBatchedReorgCheckpoint("code-172", "title-821");
        Check.Equal(item.Status, BatchedReorgCheckpointStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedReorgCheckpointMovesToRevoked()
    {
        var item = BatchedReorgCheckpoint.NewBatchedReorgCheckpoint("code-172", "title-821");
        try
        {
            item.MoveTo(BatchedReorgCheckpointStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedReorgCheckpointStatus.Revoked, "status");
    }

    public static void BatchedReorgCheckpointRejectsMovesFromExpired()
    {
        var item = BatchedReorgCheckpoint.NewBatchedReorgCheckpoint("code-172", "title-821");
        item.Status = BatchedReorgCheckpointStatus.Expired;
        Check.Fails(() => item.MoveTo(BatchedReorgCheckpointStatus.Requested), "move");
    }

    public static void RunAll()
    {
        BatchedReorgCheckpointStartsInRequested();
        BatchedReorgCheckpointMovesToRevoked();
        BatchedReorgCheckpointRejectsMovesFromExpired();
    }
}
