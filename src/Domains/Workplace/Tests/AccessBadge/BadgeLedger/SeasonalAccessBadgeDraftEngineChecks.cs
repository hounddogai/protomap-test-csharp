namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeLedger;

using Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

/// <summary>Checks of badge ledger code, run by the project's runner.</summary>
public static class SeasonalAccessBadgeDraftEngineChecks
{
    public static void SeasonalAccessBadgeDraftEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalAccessBadgeDraftEngine.DefaultSeasonalAccessBadgeDraftEngine();
        var item = SeasonalAccessBadgeDraft.NewSeasonalAccessBadgeDraft("code-642", "title-717");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalAccessBadgeDraftEngineKeepsResultsWithinBounds();
    }
}
