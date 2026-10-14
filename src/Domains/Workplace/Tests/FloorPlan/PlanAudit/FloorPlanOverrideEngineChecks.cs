namespace Acme.Workforce.Workplace.Tests.FloorPlan.PlanAudit;

using Acme.Workforce.Workplace.FloorPlan.PlanAudit;

/// <summary>Checks of plan audit code, run by the project's runner.</summary>
public static class FloorPlanOverrideEngineChecks
{
    public static void FloorPlanOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = FloorPlanOverrideEngine.DefaultFloorPlanOverrideEngine();
        var item = FloorPlanOverride.NewFloorPlanOverride("code-935", "label-215");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FloorPlanOverrideEngineKeepsResultsWithinBounds();
    }
}
