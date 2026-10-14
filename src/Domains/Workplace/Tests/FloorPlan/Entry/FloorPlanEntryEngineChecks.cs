namespace Acme.Workforce.Workplace.Tests.FloorPlan.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.FloorPlan.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class FloorPlanEntryEngineChecks
{
    public static void FloorPlanEntryEngineKeepsResultsWithinBounds()
    {
        var engine = FloorPlanEntryEngine.DefaultFloorPlanEntryEngine(Flags.NewRegistry());
        var item = FloorPlanEntry.NewFloorPlanEntry("code-465", "name-482");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FloorPlanEntryEngineKeepsResultsWithinBounds();
    }
}
