namespace Acme.Workforce.Workplace.Tests.Occupancy.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Occupancy.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class ScheduledOccupancySnapshotEngineChecks
{
    public static void ScheduledOccupancySnapshotEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledOccupancySnapshotEngine.DefaultScheduledOccupancySnapshotEngine(Flags.NewRegistry());
        var item = ScheduledOccupancySnapshot.NewScheduledOccupancySnapshot("code-530", "name-431");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledOccupancySnapshotEngineKeepsResultsWithinBounds();
    }
}
