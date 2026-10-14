namespace Acme.Workforce.Workplace.Tests.FloorPlan.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.FloorPlan.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class DefaultFloorPlanBaselineEngineChecks
{
    public static void DefaultFloorPlanBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultFloorPlanBaselineEngine.DefaultDefaultFloorPlanBaselineEngine(Flags.NewRegistry());
        var item = DefaultFloorPlanBaseline.NewDefaultFloorPlanBaseline("code-376", "label-73");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultFloorPlanBaselineEngineKeepsResultsWithinBounds();
    }
}
