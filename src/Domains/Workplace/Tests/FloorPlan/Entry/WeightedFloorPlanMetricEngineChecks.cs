namespace Acme.Workforce.Workplace.Tests.FloorPlan.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.FloorPlan.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class WeightedFloorPlanMetricEngineChecks
{
    public static void WeightedFloorPlanMetricEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedFloorPlanMetricEngine.DefaultWeightedFloorPlanMetricEngine(Flags.NewRegistry());
        var item = WeightedFloorPlanMetric.NewWeightedFloorPlanMetric("code-885", "label-710");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedFloorPlanMetricEngineKeepsResultsWithinBounds();
    }
}
