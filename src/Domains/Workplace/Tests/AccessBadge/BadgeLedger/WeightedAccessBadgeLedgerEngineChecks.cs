namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeLedger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

/// <summary>Checks of badge ledger code, run by the project's runner.</summary>
public static class WeightedAccessBadgeLedgerEngineChecks
{
    public static void WeightedAccessBadgeLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedAccessBadgeLedgerEngine.DefaultWeightedAccessBadgeLedgerEngine(Flags.NewRegistry());
        var item = WeightedAccessBadgeLedger.NewWeightedAccessBadgeLedger("code-141", "name-831");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedAccessBadgeLedgerEngineKeepsResultsWithinBounds();
    }
}
