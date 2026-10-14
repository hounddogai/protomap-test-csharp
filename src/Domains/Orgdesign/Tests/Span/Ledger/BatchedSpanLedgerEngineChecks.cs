namespace Acme.Workforce.Orgdesign.Tests.Span.Ledger;

using Acme.Workforce.Orgdesign.Span.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class BatchedSpanLedgerEngineChecks
{
    public static void BatchedSpanLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedSpanLedgerEngine.DefaultBatchedSpanLedgerEngine();
        var item = BatchedSpanLedger.NewBatchedSpanLedger("code-286", "title-64");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedSpanLedgerEngineKeepsResultsWithinBounds();
    }
}
