namespace Acme.Workforce.Absence.Tests.Accrual.Digest;

using Acme.Workforce.Absence.Accrual.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class TieredAccrualLedgerEngineChecks
{
    public static void TieredAccrualLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = TieredAccrualLedgerEngine.DefaultTieredAccrualLedgerEngine();
        var item = TieredAccrualLedger.NewTieredAccrualLedger("code-274", "name-321");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredAccrualLedgerEngineKeepsResultsWithinBounds();
    }
}
