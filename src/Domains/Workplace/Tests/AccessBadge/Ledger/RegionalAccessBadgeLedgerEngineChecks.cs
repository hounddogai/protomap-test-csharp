namespace Acme.Workforce.Workplace.Tests.AccessBadge.Ledger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.AccessBadge.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class RegionalAccessBadgeLedgerEngineChecks
{
    public static void RegionalAccessBadgeLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalAccessBadgeLedgerEngine.DefaultRegionalAccessBadgeLedgerEngine(Flags.NewRegistry());
        var item = RegionalAccessBadgeLedger.NewRegionalAccessBadgeLedger("code-78", "label-959");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalAccessBadgeLedgerEngineKeepsResultsWithinBounds();
    }
}
