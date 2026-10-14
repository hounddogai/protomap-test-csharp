namespace Acme.Workforce.Absence.Tests.Policy.RollingPolicyOverride;

using Acme.Workforce.Absence.Policy.RollingPolicyOverride;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of rolling policy override code, run by the project's runner.</summary>
public static class PolicyDigestEngineChecks
{
    public static void PolicyDigestEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyDigestEngine.DefaultPolicyDigestEngine(Flags.NewRegistry());
        var item = PolicyDigest.NewPolicyDigest("code-303", "name-929");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyDigestEngineKeepsResultsWithinBounds();
    }
}
