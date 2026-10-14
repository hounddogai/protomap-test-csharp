namespace Acme.Workforce.Compliance.Tests.PolicyAck.Claim;

using Acme.Workforce.Compliance.PolicyAck.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class GlobalPolicyAckBundleEngineChecks
{
    public static void GlobalPolicyAckBundleEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalPolicyAckBundleEngine.DefaultGlobalPolicyAckBundleEngine();
        var item = GlobalPolicyAckBundle.NewGlobalPolicyAckBundle("code-987", "title-677");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalPolicyAckBundleEngineKeepsResultsWithinBounds();
    }
}
