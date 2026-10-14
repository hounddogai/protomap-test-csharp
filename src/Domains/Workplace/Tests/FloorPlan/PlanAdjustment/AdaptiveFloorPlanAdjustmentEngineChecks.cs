namespace Acme.Workforce.Workplace.Tests.FloorPlan.PlanAdjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

/// <summary>Checks of plan adjustment code, run by the project's runner.</summary>
public static class AdaptiveFloorPlanAdjustmentEngineChecks
{
    public static void AdaptiveFloorPlanAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveFloorPlanAdjustmentEngine.DefaultAdaptiveFloorPlanAdjustmentEngine(Flags.NewRegistry());
        var item = AdaptiveFloorPlanAdjustment.NewAdaptiveFloorPlanAdjustment("code-995", "label-81");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveFloorPlanAdjustmentEngineKeepsResultsWithinBounds();
    }
}
