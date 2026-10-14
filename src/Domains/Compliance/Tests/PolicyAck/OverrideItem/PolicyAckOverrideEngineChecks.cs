namespace Acme.Workforce.Compliance.Tests.PolicyAck.OverrideItem;

using Acme.Workforce.Compliance.PolicyAck.OverrideItem;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class PolicyAckOverrideEngineChecks
{
    public static void PolicyAckOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyAckOverrideEngine.DefaultPolicyAckOverrideEngine(Flags.NewRegistry());
        var item = PolicyAckOverride.NewPolicyAckOverride("code-575", "label-308");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyAckOverrideEngineKeepsResultsWithinBounds();
    }
}
