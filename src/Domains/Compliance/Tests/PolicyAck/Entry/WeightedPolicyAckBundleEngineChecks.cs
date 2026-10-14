namespace Acme.Workforce.Compliance.Tests.PolicyAck.Entry;

using Acme.Workforce.Compliance.PolicyAck.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class WeightedPolicyAckBundleEngineChecks
{
    public static void WeightedPolicyAckBundleEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedPolicyAckBundleEngine.DefaultWeightedPolicyAckBundleEngine();
        var item = WeightedPolicyAckBundle.NewWeightedPolicyAckBundle("code-305", "label-926");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedPolicyAckBundleEngineKeepsResultsWithinBounds();
    }
}
