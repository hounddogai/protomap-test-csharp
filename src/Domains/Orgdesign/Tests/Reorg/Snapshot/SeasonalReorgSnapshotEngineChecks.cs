namespace Acme.Workforce.Orgdesign.Tests.Reorg.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Reorg.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SeasonalReorgSnapshotEngineChecks
{
    public static void SeasonalReorgSnapshotEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalReorgSnapshotEngine.DefaultSeasonalReorgSnapshotEngine(Flags.NewRegistry());
        var item = SeasonalReorgSnapshot.NewSeasonalReorgSnapshot("code-478", "label-39");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalReorgSnapshotEngineKeepsResultsWithinBounds();
    }
}
