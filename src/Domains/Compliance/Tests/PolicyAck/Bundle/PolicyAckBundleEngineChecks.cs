namespace Acme.Workforce.Compliance.Tests.PolicyAck.Bundle;

using Acme.Workforce.Compliance.PolicyAck.Bundle;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PolicyAckBundleEngineChecks
{
    public static void PolicyAckBundleEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyAckBundleEngine.DefaultPolicyAckBundleEngine(Flags.NewRegistry());
        var item = PolicyAckBundle.NewPolicyAckBundle("code-135", "name-803");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyAckBundleEngineKeepsResultsWithinBounds();
    }
}
