namespace Acme.Workforce.Workplace.Tests.Desk.Adjustment;

using Acme.Workforce.Workplace.Desk.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class FallbackDeskLedgerEngineChecks
{
    public static void FallbackDeskLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackDeskLedgerEngine.DefaultFallbackDeskLedgerEngine();
        var item = FallbackDeskLedger.NewFallbackDeskLedger("code-993", "name-293");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackDeskLedgerEngineKeepsResultsWithinBounds();
    }
}
