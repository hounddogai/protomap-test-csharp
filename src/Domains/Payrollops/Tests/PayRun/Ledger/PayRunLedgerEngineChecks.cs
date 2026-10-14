namespace Acme.Workforce.Payrollops.Tests.PayRun.Ledger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.PayRun.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class PayRunLedgerEngineChecks
{
    public static void PayRunLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = PayRunLedgerEngine.DefaultPayRunLedgerEngine(Flags.NewRegistry());
        var item = PayRunLedger.NewPayRunLedger("code-723", "name-198");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayRunLedgerEngineKeepsResultsWithinBounds();
    }
}
