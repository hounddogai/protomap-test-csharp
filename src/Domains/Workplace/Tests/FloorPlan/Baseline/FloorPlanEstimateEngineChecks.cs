namespace Acme.Workforce.Workplace.Tests.FloorPlan.Baseline;

using Acme.Workforce.Workplace.FloorPlan.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class FloorPlanEstimateEngineChecks
{
    public static void FloorPlanEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = FloorPlanEstimateEngine.DefaultFloorPlanEstimateEngine();
        var item = FloorPlanEstimate.NewFloorPlanEstimate("code-629", "label-954");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FloorPlanEstimateEngineKeepsResultsWithinBounds();
    }
}
