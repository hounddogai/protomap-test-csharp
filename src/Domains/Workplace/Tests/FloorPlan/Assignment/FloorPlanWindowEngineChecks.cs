namespace Acme.Workforce.Workplace.Tests.FloorPlan.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.FloorPlan.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class FloorPlanWindowEngineChecks
{
    public static void FloorPlanWindowEngineKeepsResultsWithinBounds()
    {
        var engine = FloorPlanWindowEngine.DefaultFloorPlanWindowEngine(Flags.NewRegistry());
        var item = FloorPlanWindow.NewFloorPlanWindow("code-592", "name-638");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FloorPlanWindowEngineKeepsResultsWithinBounds();
    }
}
