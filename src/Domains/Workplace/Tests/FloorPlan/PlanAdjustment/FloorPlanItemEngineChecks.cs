namespace Acme.Workforce.Workplace.Tests.FloorPlan.PlanAdjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

/// <summary>Checks of plan adjustment code, run by the project's runner.</summary>
public static class FloorPlanItemEngineChecks
{
    public static void FloorPlanItemEngineKeepsResultsWithinBounds()
    {
        var engine = FloorPlanItemEngine.DefaultFloorPlanItemEngine(Flags.NewRegistry());
        var item = FloorPlanItem.NewFloorPlanItem("code-733", "name-357");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FloorPlanItemEngineKeepsResultsWithinBounds();
    }
}
