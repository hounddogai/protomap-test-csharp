namespace Acme.Workforce.Compliance.Tests.PolicyAck.Profile;

using Acme.Workforce.Compliance.PolicyAck.Profile;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PolicyAckItemEngineChecks
{
    public static void PolicyAckItemEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyAckItemEngine.DefaultPolicyAckItemEngine(Flags.NewRegistry());
        var item = PolicyAckItem.NewPolicyAckItem("code-191", "label-695");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyAckItemEngineKeepsResultsWithinBounds();
    }
}
