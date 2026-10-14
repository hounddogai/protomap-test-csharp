namespace Acme.Workforce.Orgdesign.Tests.Span.Rollup;

using Acme.Workforce.Orgdesign.Span.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SpanScoreViewChecks
{
    public static void SpanScoreViewShowsTheStatusInCapitals()
    {
        var item = SpanScore.NewSpanScore("code-175", "title-609");
        var view = SpanScoreView.ToSpanScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SpanScoreView.ToSpanScoreViews(new List<SpanScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SpanScoreViewShowsTheStatusInCapitals();
    }
}
