namespace Acme.Workforce.Absence.Tests.Policy.PolicyRollup;

using Acme.Workforce.Absence.Policy.PolicyRollup;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of policy rollup code, run by the project's runner.</summary>
public static class AdaptivePolicyOverrideEngineChecks
{
    public static void AdaptivePolicyOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptivePolicyOverrideEngine.DefaultAdaptivePolicyOverrideEngine(Flags.NewRegistry());
        var item = AdaptivePolicyOverride.NewAdaptivePolicyOverride("code-898", "name-580");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptivePolicyOverrideEngineKeepsResultsWithinBounds();
    }
}
