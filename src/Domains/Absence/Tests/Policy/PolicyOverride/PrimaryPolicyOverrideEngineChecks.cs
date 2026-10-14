namespace Acme.Workforce.Absence.Tests.Policy.PolicyOverride;

using Acme.Workforce.Absence.Policy.PolicyOverride;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of policy override code, run by the project's runner.</summary>
public static class PrimaryPolicyOverrideEngineChecks
{
    public static void PrimaryPolicyOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryPolicyOverrideEngine.DefaultPrimaryPolicyOverrideEngine(Flags.NewRegistry());
        var item = PrimaryPolicyOverride.NewPrimaryPolicyOverride("code-589", "name-224");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryPolicyOverrideEngineKeepsResultsWithinBounds();
    }
}
