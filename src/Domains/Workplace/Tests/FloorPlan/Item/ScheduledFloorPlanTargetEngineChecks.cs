namespace Acme.Workforce.Workplace.Tests.FloorPlan.Item;

using Acme.Workforce.Workplace.FloorPlan.Item;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class ScheduledFloorPlanTargetEngineChecks
{
    public static void ScheduledFloorPlanTargetEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledFloorPlanTargetEngine.DefaultScheduledFloorPlanTargetEngine();
        var item = ScheduledFloorPlanTarget.NewScheduledFloorPlanTarget("code-715", "label-187");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledFloorPlanTargetEngineKeepsResultsWithinBounds();
    }
}
