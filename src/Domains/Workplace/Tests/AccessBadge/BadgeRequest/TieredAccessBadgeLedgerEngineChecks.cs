namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeRequest;

using Acme.Workforce.Workplace.AccessBadge.BadgeRequest;

/// <summary>Checks of badge request code, run by the project's runner.</summary>
public static class TieredAccessBadgeLedgerEngineChecks
{
    public static void TieredAccessBadgeLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = TieredAccessBadgeLedgerEngine.DefaultTieredAccessBadgeLedgerEngine();
        var item = TieredAccessBadgeLedger.NewTieredAccessBadgeLedger("code-282", "label-376");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredAccessBadgeLedgerEngineKeepsResultsWithinBounds();
    }
}
