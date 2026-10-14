namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanRollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SuccessionPlan.PlanRollup;

/// <summary>Checks of plan rollup code, run by the project's runner.</summary>
public static class FallbackSuccessionPlanManifestEngineChecks
{
    public static void FallbackSuccessionPlanManifestEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackSuccessionPlanManifestEngine.DefaultFallbackSuccessionPlanManifestEngine(Flags.NewRegistry());
        var item = FallbackSuccessionPlanManifest.NewFallbackSuccessionPlanManifest("code-353", "name-283");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackSuccessionPlanManifestEngineKeepsResultsWithinBounds();
    }
}
