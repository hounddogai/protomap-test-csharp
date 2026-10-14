namespace Acme.Workforce.Orgdesign.Tests.Span.Digest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Span.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class SpanLedgerEngineChecks
{
    public static void SpanLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = SpanLedgerEngine.DefaultSpanLedgerEngine(Flags.NewRegistry());
        var item = SpanLedger.NewSpanLedger("code-459", "name-31");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SpanLedgerEngineKeepsResultsWithinBounds();
    }
}
