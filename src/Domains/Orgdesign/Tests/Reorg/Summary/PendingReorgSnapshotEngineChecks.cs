namespace Acme.Workforce.Orgdesign.Tests.Reorg.Summary;

using Acme.Workforce.Orgdesign.Reorg.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PendingReorgSnapshotEngineChecks
{
    public static void PendingReorgSnapshotEngineKeepsResultsWithinBounds()
    {
        var engine = PendingReorgSnapshotEngine.DefaultPendingReorgSnapshotEngine();
        var item = PendingReorgSnapshot.NewPendingReorgSnapshot("code-788", "label-433");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingReorgSnapshotEngineKeepsResultsWithinBounds();
    }
}
