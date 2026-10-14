namespace Acme.Workforce.Payrollops.Tests.PayRun.GlobalRunLedger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.PayRun.GlobalRunLedger;

/// <summary>Checks of global run ledger code, run by the project's runner.</summary>
public static class LenientPayRunLedgerEngineChecks
{
    public static void LenientPayRunLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = LenientPayRunLedgerEngine.DefaultLenientPayRunLedgerEngine(Flags.NewRegistry());
        var item = LenientPayRunLedger.NewLenientPayRunLedger("code-975", "name-288");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientPayRunLedgerEngineKeepsResultsWithinBounds();
    }
}
