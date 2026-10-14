namespace Acme.Workforce.Talent.Tests.Nomination.Adjustment;

using Acme.Workforce.Talent.Nomination.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class GlobalNominationSnapshotEngineChecks
{
    public static void GlobalNominationSnapshotEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalNominationSnapshotEngine.DefaultGlobalNominationSnapshotEngine();
        var item = GlobalNominationSnapshot.NewGlobalNominationSnapshot("code-672", "title-438");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalNominationSnapshotEngineKeepsResultsWithinBounds();
    }
}
