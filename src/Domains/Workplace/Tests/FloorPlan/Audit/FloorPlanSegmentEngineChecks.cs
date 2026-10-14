namespace Acme.Workforce.Workplace.Tests.FloorPlan.Audit;

using Acme.Workforce.Workplace.FloorPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class FloorPlanSegmentEngineChecks
{
    public static void FloorPlanSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = FloorPlanSegmentEngine.DefaultFloorPlanSegmentEngine();
        var item = FloorPlanSegment.NewFloorPlanSegment("code-629", "title-649");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FloorPlanSegmentEngineKeepsResultsWithinBounds();
    }
}
