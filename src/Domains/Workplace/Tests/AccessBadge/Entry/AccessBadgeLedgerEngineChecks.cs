namespace Acme.Workforce.Workplace.Tests.AccessBadge.Entry;

using Acme.Workforce.Workplace.AccessBadge.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class AccessBadgeLedgerEngineChecks
{
    public static void AccessBadgeLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = AccessBadgeLedgerEngine.DefaultAccessBadgeLedgerEngine();
        var item = AccessBadgeLedger.NewAccessBadgeLedger("code-558", "label-682");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccessBadgeLedgerEngineKeepsResultsWithinBounds();
    }
}
