namespace Acme.Workforce.Workplace.Tests.FloorPlan.PlanAudit;

using Acme.Workforce.Workplace.FloorPlan.PlanAudit;

/// <summary>Checks of plan audit code, run by the project's runner.</summary>
public static class DefaultFloorPlanSegmentEngineChecks
{
    public static void DefaultFloorPlanSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultFloorPlanSegmentEngine.DefaultDefaultFloorPlanSegmentEngine();
        var item = DefaultFloorPlanSegment.NewDefaultFloorPlanSegment("code-349", "name-966");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultFloorPlanSegmentEngineKeepsResultsWithinBounds();
    }
}
