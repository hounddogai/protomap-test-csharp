namespace Acme.Workforce.Compliance.Tests.PolicyAck.Quota;

using Acme.Workforce.Compliance.PolicyAck.Quota;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class PolicyAckQuotaEngineChecks
{
    public static void PolicyAckQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyAckQuotaEngine.DefaultPolicyAckQuotaEngine(Flags.NewRegistry());
        var item = PolicyAckQuota.NewPolicyAckQuota("code-663", "title-934");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyAckQuotaEngineKeepsResultsWithinBounds();
    }
}
