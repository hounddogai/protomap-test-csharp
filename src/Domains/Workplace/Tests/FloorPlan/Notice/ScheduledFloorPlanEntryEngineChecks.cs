namespace Acme.Workforce.Workplace.Tests.FloorPlan.Notice;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.FloorPlan.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class ScheduledFloorPlanEntryEngineChecks
{
    public static void ScheduledFloorPlanEntryEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledFloorPlanEntryEngine.DefaultScheduledFloorPlanEntryEngine(Flags.NewRegistry());
        var item = ScheduledFloorPlanEntry.NewScheduledFloorPlanEntry("code-708", "title-367");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledFloorPlanEntryEngineKeepsResultsWithinBounds();
    }
}
