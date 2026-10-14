namespace Acme.Workforce.Absence.Tests.Policy.PolicyOverride;

using Acme.Workforce.Absence.Policy.PolicyOverride;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of policy override code, run by the project's runner.</summary>
public static class RollingPolicyOverrideEngineChecks
{
    public static void RollingPolicyOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = RollingPolicyOverrideEngine.DefaultRollingPolicyOverrideEngine(Flags.NewRegistry());
        var item = RollingPolicyOverride.NewRollingPolicyOverride("code-598", "name-579");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingPolicyOverrideEngineKeepsResultsWithinBounds();
    }
}
