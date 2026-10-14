namespace Acme.Workforce.Compensation.Tests.EquityGrant.OverrideItem;

using Acme.Workforce.Compensation.EquityGrant.OverrideItem;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class TieredEquityGrantLedgerEngineChecks
{
    public static void TieredEquityGrantLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = TieredEquityGrantLedgerEngine.DefaultTieredEquityGrantLedgerEngine(Flags.NewRegistry());
        var item = TieredEquityGrantLedger.NewTieredEquityGrantLedger("code-120", "name-257");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredEquityGrantLedgerEngineKeepsResultsWithinBounds();
    }
}
