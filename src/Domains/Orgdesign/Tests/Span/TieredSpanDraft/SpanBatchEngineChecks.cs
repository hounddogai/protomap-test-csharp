namespace Acme.Workforce.Orgdesign.Tests.Span.TieredSpanDraft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

/// <summary>Checks of tiered span draft code, run by the project's runner.</summary>
public static class SpanBatchEngineChecks
{
    public static void SpanBatchEngineKeepsResultsWithinBounds()
    {
        var engine = SpanBatchEngine.DefaultSpanBatchEngine(Flags.NewRegistry());
        var item = SpanBatch.NewSpanBatch("code-465", "label-815");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SpanBatchEngineKeepsResultsWithinBounds();
    }
}
