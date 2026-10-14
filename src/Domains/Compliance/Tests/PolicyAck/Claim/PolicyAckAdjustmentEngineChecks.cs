namespace Acme.Workforce.Compliance.Tests.PolicyAck.Claim;

using Acme.Workforce.Compliance.PolicyAck.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class PolicyAckAdjustmentEngineChecks
{
    public static void PolicyAckAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyAckAdjustmentEngine.DefaultPolicyAckAdjustmentEngine();
        var item = PolicyAckAdjustment.NewPolicyAckAdjustment("code-651", "title-440");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyAckAdjustmentEngineKeepsResultsWithinBounds();
    }
}
