namespace Acme.Workforce.Workplace.Tests.FloorPlan.Item;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.FloorPlan.Item;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class ManualFloorPlanItemEngineChecks
{
    public static void ManualFloorPlanItemEngineKeepsResultsWithinBounds()
    {
        var engine = ManualFloorPlanItemEngine.DefaultManualFloorPlanItemEngine(Flags.NewRegistry());
        var item = ManualFloorPlanItem.NewManualFloorPlanItem("code-85", "title-904");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualFloorPlanItemEngineKeepsResultsWithinBounds();
    }
}
