namespace Acme.Workforce.Compliance.Tests.PolicyAck.Claim;

using Acme.Workforce.Compliance.PolicyAck.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class BatchedPolicyAckSnapshotEngineChecks
{
    public static void BatchedPolicyAckSnapshotEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedPolicyAckSnapshotEngine.DefaultBatchedPolicyAckSnapshotEngine();
        var item = BatchedPolicyAckSnapshot.NewBatchedPolicyAckSnapshot("code-656", "title-540");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedPolicyAckSnapshotEngineKeepsResultsWithinBounds();
    }
}
