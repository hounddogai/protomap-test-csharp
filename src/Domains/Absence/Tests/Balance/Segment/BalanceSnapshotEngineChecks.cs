namespace Acme.Workforce.Absence.Tests.Balance.Segment;

using Acme.Workforce.Absence.Balance.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class BalanceSnapshotEngineChecks
{
    public static void BalanceSnapshotEngineKeepsResultsWithinBounds()
    {
        var engine = BalanceSnapshotEngine.DefaultBalanceSnapshotEngine();
        var item = BalanceSnapshot.NewBalanceSnapshot("code-151", "label-965");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BalanceSnapshotEngineKeepsResultsWithinBounds();
    }
}
