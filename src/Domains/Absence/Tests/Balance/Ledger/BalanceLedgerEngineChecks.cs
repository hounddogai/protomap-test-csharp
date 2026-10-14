namespace Acme.Workforce.Absence.Tests.Balance.Ledger;

using Acme.Workforce.Absence.Balance.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class BalanceLedgerEngineChecks
{
    public static void BalanceLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = BalanceLedgerEngine.DefaultBalanceLedgerEngine();
        var item = BalanceLedger.NewBalanceLedger("code-500", "label-216");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BalanceLedgerEngineKeepsResultsWithinBounds();
    }
}
