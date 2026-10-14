namespace Acme.Workforce.Orgdesign.Tests.Span.SpanReview;

using Acme.Workforce.Orgdesign.Span.SpanReview;

/// <summary>Checks of span review code, run by the project's runner.</summary>
public static class RegionalSpanReviewEngineChecks
{
    public static void RegionalSpanReviewEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalSpanReviewEngine.DefaultRegionalSpanReviewEngine();
        var item = RegionalSpanReview.NewRegionalSpanReview("code-185", "label-80");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalSpanReviewEngineKeepsResultsWithinBounds();
    }
}
