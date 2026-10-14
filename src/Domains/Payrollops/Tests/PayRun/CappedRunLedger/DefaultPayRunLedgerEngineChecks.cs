namespace Acme.Workforce.Payrollops.Tests.PayRun.CappedRunLedger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

/// <summary>Checks of capped run ledger code, run by the project's runner.</summary>
public static class DefaultPayRunLedgerEngineChecks
{
    public static void DefaultPayRunLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultPayRunLedgerEngine.DefaultDefaultPayRunLedgerEngine(Flags.NewRegistry());
        var item = DefaultPayRunLedger.NewDefaultPayRunLedger("code-455", "title-272");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultPayRunLedgerEngineKeepsResultsWithinBounds();
    }
}
