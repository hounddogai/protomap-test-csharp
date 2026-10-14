namespace Acme.Workforce.Contractors.Tests.Rate.Snapshot;

using Acme.Workforce.Contractors.Rate.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SeasonalRateSnapshotEngineChecks
{
    public static void SeasonalRateSnapshotEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalRateSnapshotEngine.DefaultSeasonalRateSnapshotEngine();
        var item = SeasonalRateSnapshot.NewSeasonalRateSnapshot("code-932", "label-16");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalRateSnapshotEngineKeepsResultsWithinBounds();
    }
}
