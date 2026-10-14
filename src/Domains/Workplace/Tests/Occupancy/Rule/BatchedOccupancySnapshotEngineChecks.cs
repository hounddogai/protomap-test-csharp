namespace Acme.Workforce.Workplace.Tests.Occupancy.Rule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Occupancy.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class BatchedOccupancySnapshotEngineChecks
{
    public static void BatchedOccupancySnapshotEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedOccupancySnapshotEngine.DefaultBatchedOccupancySnapshotEngine(Flags.NewRegistry());
        var item = BatchedOccupancySnapshot.NewBatchedOccupancySnapshot("code-118", "title-758");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedOccupancySnapshotEngineKeepsResultsWithinBounds();
    }
}
