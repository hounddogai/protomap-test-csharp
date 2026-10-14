namespace Acme.Workforce.Workplace.Tests.FloorPlan.Item;

using Acme.Workforce.Workplace.FloorPlan.Item;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class FloorPlanSlotEngineChecks
{
    public static void FloorPlanSlotEngineKeepsResultsWithinBounds()
    {
        var engine = FloorPlanSlotEngine.DefaultFloorPlanSlotEngine();
        var item = FloorPlanSlot.NewFloorPlanSlot("code-745", "label-290");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FloorPlanSlotEngineKeepsResultsWithinBounds();
    }
}
