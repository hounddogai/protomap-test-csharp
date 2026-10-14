namespace Acme.Workforce.Workplace.Tests.FloorPlan.Audit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.FloorPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class FallbackFloorPlanRequestEngineChecks
{
    public static void FallbackFloorPlanRequestEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackFloorPlanRequestEngine.DefaultFallbackFloorPlanRequestEngine(Flags.NewRegistry());
        var item = FallbackFloorPlanRequest.NewFallbackFloorPlanRequest("code-219", "label-774");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackFloorPlanRequestEngineKeepsResultsWithinBounds();
    }
}
