namespace Acme.Workforce.Workplace.Tests.FloorPlan.Entry;

using Acme.Workforce.Workplace.FloorPlan.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class FloorPlanGroupEngineChecks
{
    public static void FloorPlanGroupEngineKeepsResultsWithinBounds()
    {
        var engine = FloorPlanGroupEngine.DefaultFloorPlanGroupEngine();
        var item = FloorPlanGroup.NewFloorPlanGroup("code-5", "label-78");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FloorPlanGroupEngineKeepsResultsWithinBounds();
    }
}
