namespace Acme.Workforce.Orgdesign.Tests.Span.TieredSpanDraft;

using Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

/// <summary>Checks of tiered span draft code, run by the project's runner.</summary>
public static class SpanEstimateEngineChecks
{
    public static void SpanEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = SpanEstimateEngine.DefaultSpanEstimateEngine();
        var item = SpanEstimate.NewSpanEstimate("code-978", "name-708");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SpanEstimateEngineKeepsResultsWithinBounds();
    }
}
