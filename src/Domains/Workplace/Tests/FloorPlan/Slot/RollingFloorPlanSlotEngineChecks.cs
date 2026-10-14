namespace Acme.Workforce.Workplace.Tests.FloorPlan.Slot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.FloorPlan.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class RollingFloorPlanSlotEngineChecks
{
    public static void RollingFloorPlanSlotEngineKeepsResultsWithinBounds()
    {
        var engine = RollingFloorPlanSlotEngine.DefaultRollingFloorPlanSlotEngine(Flags.NewRegistry());
        var item = RollingFloorPlanSlot.NewRollingFloorPlanSlot("code-924", "title-49");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingFloorPlanSlotEngineKeepsResultsWithinBounds();
    }
}
