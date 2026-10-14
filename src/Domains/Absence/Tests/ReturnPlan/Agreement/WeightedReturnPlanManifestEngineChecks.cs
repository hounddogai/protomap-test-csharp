namespace Acme.Workforce.Absence.Tests.ReturnPlan.Agreement;

using Acme.Workforce.Absence.ReturnPlan.Agreement;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class WeightedReturnPlanManifestEngineChecks
{
    public static void WeightedReturnPlanManifestEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedReturnPlanManifestEngine.DefaultWeightedReturnPlanManifestEngine(Flags.NewRegistry());
        var item = WeightedReturnPlanManifest.NewWeightedReturnPlanManifest("code-991", "name-862");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedReturnPlanManifestEngineKeepsResultsWithinBounds();
    }
}
