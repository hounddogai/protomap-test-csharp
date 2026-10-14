namespace Acme.Workforce.Compensation.Tests.EquityGrant.RecordItem;

using Acme.Workforce.Compensation.EquityGrant.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LenientEquityGrantLedgerEngineChecks
{
    public static void LenientEquityGrantLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = LenientEquityGrantLedgerEngine.DefaultLenientEquityGrantLedgerEngine();
        var item = LenientEquityGrantLedger.NewLenientEquityGrantLedger("code-416", "title-25");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientEquityGrantLedgerEngineKeepsResultsWithinBounds();
    }
}
