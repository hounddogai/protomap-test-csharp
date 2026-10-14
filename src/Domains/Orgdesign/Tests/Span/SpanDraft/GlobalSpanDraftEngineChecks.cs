namespace Acme.Workforce.Orgdesign.Tests.Span.SpanDraft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Span.SpanDraft;

/// <summary>Checks of span draft code, run by the project's runner.</summary>
public static class GlobalSpanDraftEngineChecks
{
    public static void GlobalSpanDraftEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalSpanDraftEngine.DefaultGlobalSpanDraftEngine(Flags.NewRegistry());
        var item = GlobalSpanDraft.NewGlobalSpanDraft("code-907", "title-833");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalSpanDraftEngineKeepsResultsWithinBounds();
    }
}
