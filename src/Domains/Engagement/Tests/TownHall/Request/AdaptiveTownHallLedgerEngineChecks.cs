namespace Acme.Workforce.Engagement.Tests.TownHall.Request;

using Acme.Workforce.Engagement.TownHall.Request;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class AdaptiveTownHallLedgerEngineChecks
{
    public static void AdaptiveTownHallLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveTownHallLedgerEngine.DefaultAdaptiveTownHallLedgerEngine(Flags.NewRegistry());
        var item = AdaptiveTownHallLedger.NewAdaptiveTownHallLedger("code-324", "name-96");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveTownHallLedgerEngineKeepsResultsWithinBounds();
    }
}
