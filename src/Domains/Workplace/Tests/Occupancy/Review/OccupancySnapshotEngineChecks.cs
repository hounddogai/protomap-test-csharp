namespace Acme.Workforce.Workplace.Tests.Occupancy.Review;

using Acme.Workforce.Workplace.Occupancy.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class OccupancySnapshotEngineChecks
{
    public static void OccupancySnapshotEngineKeepsResultsWithinBounds()
    {
        var engine = OccupancySnapshotEngine.DefaultOccupancySnapshotEngine();
        var item = OccupancySnapshot.NewOccupancySnapshot("code-289", "title-96");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        OccupancySnapshotEngineKeepsResultsWithinBounds();
    }
}
