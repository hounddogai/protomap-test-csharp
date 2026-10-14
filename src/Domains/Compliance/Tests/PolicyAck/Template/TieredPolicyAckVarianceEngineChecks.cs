namespace Acme.Workforce.Compliance.Tests.PolicyAck.Template;

using Acme.Workforce.Compliance.PolicyAck.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class TieredPolicyAckVarianceEngineChecks
{
    public static void TieredPolicyAckVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = TieredPolicyAckVarianceEngine.DefaultTieredPolicyAckVarianceEngine();
        var item = TieredPolicyAckVariance.NewTieredPolicyAckVariance("code-911", "label-24");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredPolicyAckVarianceEngineKeepsResultsWithinBounds();
    }
}
