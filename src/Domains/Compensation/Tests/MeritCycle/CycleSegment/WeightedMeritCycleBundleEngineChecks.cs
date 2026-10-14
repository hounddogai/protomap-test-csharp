namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleSegment;

using Acme.Workforce.Compensation.MeritCycle.CycleSegment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of cycle segment code, run by the project's runner.</summary>
public static class WeightedMeritCycleBundleEngineChecks
{
    public static void WeightedMeritCycleBundleEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedMeritCycleBundleEngine.DefaultWeightedMeritCycleBundleEngine(Flags.NewRegistry());
        var item = WeightedMeritCycleBundle.NewWeightedMeritCycleBundle("code-408", "label-474");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedMeritCycleBundleEngineKeepsResultsWithinBounds();
    }
}
