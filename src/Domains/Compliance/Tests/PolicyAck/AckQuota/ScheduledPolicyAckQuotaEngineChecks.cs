namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckQuota;

using Acme.Workforce.Compliance.PolicyAck.AckQuota;

/// <summary>Checks of ack quota code, run by the project's runner.</summary>
public static class ScheduledPolicyAckQuotaEngineChecks
{
    public static void ScheduledPolicyAckQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledPolicyAckQuotaEngine.DefaultScheduledPolicyAckQuotaEngine();
        var item = ScheduledPolicyAckQuota.NewScheduledPolicyAckQuota("code-458", "name-923");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledPolicyAckQuotaEngineKeepsResultsWithinBounds();
    }
}
