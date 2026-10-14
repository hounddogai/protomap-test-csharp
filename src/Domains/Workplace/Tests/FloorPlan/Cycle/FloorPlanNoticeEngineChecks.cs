namespace Acme.Workforce.Workplace.Tests.FloorPlan.Cycle;

using Acme.Workforce.Workplace.FloorPlan.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class FloorPlanNoticeEngineChecks
{
    public static void FloorPlanNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = FloorPlanNoticeEngine.DefaultFloorPlanNoticeEngine();
        var item = FloorPlanNotice.NewFloorPlanNotice("code-200", "title-409");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FloorPlanNoticeEngineKeepsResultsWithinBounds();
    }
}
