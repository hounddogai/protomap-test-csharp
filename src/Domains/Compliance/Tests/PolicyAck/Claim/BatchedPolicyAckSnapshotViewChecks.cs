namespace Acme.Workforce.Compliance.Tests.PolicyAck.Claim;

using Acme.Workforce.Compliance.PolicyAck.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class BatchedPolicyAckSnapshotViewChecks
{
    public static void BatchedPolicyAckSnapshotViewShowsTheStatusInCapitals()
    {
        var item = BatchedPolicyAckSnapshot.NewBatchedPolicyAckSnapshot("code-656", "title-540");
        var view = BatchedPolicyAckSnapshotView.ToBatchedPolicyAckSnapshotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedPolicyAckSnapshotView.ToBatchedPolicyAckSnapshotViews(new List<BatchedPolicyAckSnapshot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedPolicyAckSnapshotViewShowsTheStatusInCapitals();
    }
}
