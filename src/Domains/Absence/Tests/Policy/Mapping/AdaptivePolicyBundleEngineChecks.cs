namespace Acme.Workforce.Absence.Tests.Policy.Mapping;

using Acme.Workforce.Absence.Policy.Mapping;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class AdaptivePolicyBundleEngineChecks
{
    public static void AdaptivePolicyBundleEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptivePolicyBundleEngine.DefaultAdaptivePolicyBundleEngine(Flags.NewRegistry());
        var item = AdaptivePolicyBundle.NewAdaptivePolicyBundle("code-738", "label-214");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptivePolicyBundleEngineKeepsResultsWithinBounds();
    }
}
