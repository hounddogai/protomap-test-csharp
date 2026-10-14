namespace Acme.Workforce.Engagement.Tests.TownHall.Ledger;

using Acme.Workforce.Engagement.TownHall.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class AdaptiveTownHallNoticeEngineChecks
{
    public static void AdaptiveTownHallNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveTownHallNoticeEngine.DefaultAdaptiveTownHallNoticeEngine();
        var item = AdaptiveTownHallNotice.NewAdaptiveTownHallNotice("code-612", "title-436");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveTownHallNoticeEngineKeepsResultsWithinBounds();
    }
}
