namespace Acme.Workforce.Workplace.Tests.FloorPlan.Cycle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.FloorPlan.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class FloorPlanCycleEngineChecks
{
    public static void FloorPlanCycleEngineKeepsResultsWithinBounds()
    {
        var engine = FloorPlanCycleEngine.DefaultFloorPlanCycleEngine(Flags.NewRegistry());
        var item = FloorPlanCycle.NewFloorPlanCycle("code-760", "label-998");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FloorPlanCycleEngineKeepsResultsWithinBounds();
    }
}
