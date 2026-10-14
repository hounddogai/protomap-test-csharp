namespace Acme.Workforce.Compliance.Tests.PolicyAck.WeightedAckGroup;

using Acme.Workforce.Compliance.PolicyAck.WeightedAckGroup;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of weighted ack group code, run by the project's runner.</summary>
public static class AdaptivePolicyAckClaimEngineChecks
{
    public static void AdaptivePolicyAckClaimEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptivePolicyAckClaimEngine.DefaultAdaptivePolicyAckClaimEngine(Flags.NewRegistry());
        var item = AdaptivePolicyAckClaim.NewAdaptivePolicyAckClaim("code-581", "label-804");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptivePolicyAckClaimEngineKeepsResultsWithinBounds();
    }
}
