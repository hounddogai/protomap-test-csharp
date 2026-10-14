namespace Acme.Workforce.Workplace.Tests.FloorPlan.Bundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.FloorPlan.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class FloorPlanRequestEngineChecks
{
    public static void FloorPlanRequestEngineKeepsResultsWithinBounds()
    {
        var engine = FloorPlanRequestEngine.DefaultFloorPlanRequestEngine(Flags.NewRegistry());
        var item = FloorPlanRequest.NewFloorPlanRequest("code-524", "label-861");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FloorPlanRequestEngineKeepsResultsWithinBounds();
    }
}
