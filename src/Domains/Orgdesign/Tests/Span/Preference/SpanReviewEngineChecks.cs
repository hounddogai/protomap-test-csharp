namespace Acme.Workforce.Orgdesign.Tests.Span.Preference;

using Acme.Workforce.Orgdesign.Span.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class SpanReviewEngineChecks
{
    public static void SpanReviewEngineKeepsResultsWithinBounds()
    {
        var engine = SpanReviewEngine.DefaultSpanReviewEngine();
        var item = SpanReview.NewSpanReview("code-354", "name-160");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SpanReviewEngineKeepsResultsWithinBounds();
    }
}
