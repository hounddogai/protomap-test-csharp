namespace Acme.Workforce.Workplace.Tests.FloorPlan.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.FloorPlan.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class ScheduledFloorPlanGroupEngineChecks
{
    public static void ScheduledFloorPlanGroupEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledFloorPlanGroupEngine.DefaultScheduledFloorPlanGroupEngine(Flags.NewRegistry());
        var item = ScheduledFloorPlanGroup.NewScheduledFloorPlanGroup("code-593", "title-230");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledFloorPlanGroupEngineKeepsResultsWithinBounds();
    }
}
