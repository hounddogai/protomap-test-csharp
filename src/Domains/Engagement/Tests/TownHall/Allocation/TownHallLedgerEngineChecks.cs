namespace Acme.Workforce.Engagement.Tests.TownHall.Allocation;

using Acme.Workforce.Engagement.TownHall.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class TownHallLedgerEngineChecks
{
    public static void TownHallLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = TownHallLedgerEngine.DefaultTownHallLedgerEngine();
        var item = TownHallLedger.NewTownHallLedger("code-585", "name-493");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TownHallLedgerEngineKeepsResultsWithinBounds();
    }
}
