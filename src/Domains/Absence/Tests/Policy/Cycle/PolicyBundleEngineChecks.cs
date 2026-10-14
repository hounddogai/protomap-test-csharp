namespace Acme.Workforce.Absence.Tests.Policy.Cycle;

using Acme.Workforce.Absence.Policy.Cycle;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class PolicyBundleEngineChecks
{
    public static void PolicyBundleEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyBundleEngine.DefaultPolicyBundleEngine(Flags.NewRegistry());
        var item = PolicyBundle.NewPolicyBundle("code-832", "label-514");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyBundleEngineKeepsResultsWithinBounds();
    }
}
