namespace Acme.Workforce.Compliance.Tests.PolicyAck.WeightedAckGroup;

using Acme.Workforce.Compliance.PolicyAck.WeightedAckGroup;

/// <summary>Checks of weighted ack group code, run by the project's runner.</summary>
public static class FallbackPolicyAckPreferenceEngineChecks
{
    public static void FallbackPolicyAckPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackPolicyAckPreferenceEngine.DefaultFallbackPolicyAckPreferenceEngine();
        var item = FallbackPolicyAckPreference.NewFallbackPolicyAckPreference("code-192", "title-859");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackPolicyAckPreferenceEngineKeepsResultsWithinBounds();
    }
}
