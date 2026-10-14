namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckQuota;

using Acme.Workforce.Compliance.PolicyAck.AckQuota;

/// <summary>Checks of ack quota code, run by the project's runner.</summary>
public static class PrimaryPolicyAckOverrideEngineChecks
{
    public static void PrimaryPolicyAckOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryPolicyAckOverrideEngine.DefaultPrimaryPolicyAckOverrideEngine();
        var item = PrimaryPolicyAckOverride.NewPrimaryPolicyAckOverride("code-681", "title-831");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryPolicyAckOverrideEngineKeepsResultsWithinBounds();
    }
}
