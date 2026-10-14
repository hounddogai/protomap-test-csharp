namespace Acme.Workforce.Orgdesign.Tests.Span.SpanDraft;

using Acme.Workforce.Orgdesign.Span.SpanDraft;

/// <summary>Checks of span draft code, run by the project's runner.</summary>
public static class DefaultSpanClaimEngineChecks
{
    public static void DefaultSpanClaimEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultSpanClaimEngine.DefaultDefaultSpanClaimEngine();
        var item = DefaultSpanClaim.NewDefaultSpanClaim("code-603", "label-489");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultSpanClaimEngineKeepsResultsWithinBounds();
    }
}
