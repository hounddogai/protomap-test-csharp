namespace Acme.Workforce.Workplace.Tests.Desk.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Desk.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class FallbackDeskSnapshotEngineChecks
{
    public static void FallbackDeskSnapshotEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackDeskSnapshotEngine.DefaultFallbackDeskSnapshotEngine(Flags.NewRegistry());
        var item = FallbackDeskSnapshot.NewFallbackDeskSnapshot("code-285", "title-457");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackDeskSnapshotEngineKeepsResultsWithinBounds();
    }
}
