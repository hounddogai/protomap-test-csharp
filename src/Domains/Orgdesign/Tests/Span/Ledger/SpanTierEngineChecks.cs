namespace Acme.Workforce.Orgdesign.Tests.Span.Ledger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Span.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class SpanTierEngineChecks
{
    public static void SpanTierEngineKeepsResultsWithinBounds()
    {
        var engine = SpanTierEngine.DefaultSpanTierEngine(Flags.NewRegistry());
        var item = SpanTier.NewSpanTier("code-792", "name-347");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SpanTierEngineKeepsResultsWithinBounds();
    }
}
