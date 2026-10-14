namespace Acme.Workforce.Compliance.Tests.PolicyAck.Exception;

using Acme.Workforce.Compliance.PolicyAck.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class WeightedPolicyAckDraftEngineChecks
{
    public static void WeightedPolicyAckDraftEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedPolicyAckDraftEngine.DefaultWeightedPolicyAckDraftEngine();
        var item = WeightedPolicyAckDraft.NewWeightedPolicyAckDraft("code-73", "title-949");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedPolicyAckDraftEngineKeepsResultsWithinBounds();
    }
}
