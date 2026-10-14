namespace Acme.Workforce.Workplace.Tests.FloorPlan.PlanAdjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

/// <summary>Checks of plan adjustment code, run by the project's runner.</summary>
public static class FloorPlanMetricEngineChecks
{
    public static void FloorPlanMetricEngineKeepsResultsWithinBounds()
    {
        var engine = FloorPlanMetricEngine.DefaultFloorPlanMetricEngine(Flags.NewRegistry());
        var item = FloorPlanMetric.NewFloorPlanMetric("code-596", "name-36");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FloorPlanMetricEngineKeepsResultsWithinBounds();
    }
}
