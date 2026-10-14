namespace Acme.Workforce.Compliance.Tests.PolicyAck.Claim;

using Acme.Workforce.Compliance.PolicyAck.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class BatchedPolicyAckSnapshotChecks
{
    public static void BatchedPolicyAckSnapshotStartsInRequested()
    {
        var item = BatchedPolicyAckSnapshot.NewBatchedPolicyAckSnapshot("code-656", "title-540");
        Check.Equal(item.Status, BatchedPolicyAckSnapshotStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedPolicyAckSnapshotMovesToGranted()
    {
        var item = BatchedPolicyAckSnapshot.NewBatchedPolicyAckSnapshot("code-656", "title-540");
        try
        {
            item.MoveTo(BatchedPolicyAckSnapshotStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedPolicyAckSnapshotStatus.Granted, "status");
    }

    public static void BatchedPolicyAckSnapshotRejectsMovesFromExpired()
    {
        var item = BatchedPolicyAckSnapshot.NewBatchedPolicyAckSnapshot("code-656", "title-540");
        item.Status = BatchedPolicyAckSnapshotStatus.Expired;
        Check.Fails(() => item.MoveTo(BatchedPolicyAckSnapshotStatus.Requested), "move");
    }

    public static void RunAll()
    {
        BatchedPolicyAckSnapshotStartsInRequested();
        BatchedPolicyAckSnapshotMovesToGranted();
        BatchedPolicyAckSnapshotRejectsMovesFromExpired();
    }
}
