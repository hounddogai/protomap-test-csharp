namespace Acme.Workforce.Workplace.Tests.FloorPlan.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.FloorPlan.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class ScheduledFloorPlanSnapshotEngineChecks
{
    public static void ScheduledFloorPlanSnapshotEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledFloorPlanSnapshotEngine.DefaultScheduledFloorPlanSnapshotEngine(Flags.NewRegistry());
        var item = ScheduledFloorPlanSnapshot.NewScheduledFloorPlanSnapshot("code-704", "title-152");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledFloorPlanSnapshotEngineKeepsResultsWithinBounds();
    }
}
