namespace Acme.Workforce.Orgdesign.Tests.Reorg.Summary;

using Acme.Workforce.Orgdesign.Reorg.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class ReorgSnapshotEngineChecks
{
    public static void ReorgSnapshotEngineKeepsResultsWithinBounds()
    {
        var engine = ReorgSnapshotEngine.DefaultReorgSnapshotEngine();
        var item = ReorgSnapshot.NewReorgSnapshot("code-169", "name-298");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReorgSnapshotEngineKeepsResultsWithinBounds();
    }
}
