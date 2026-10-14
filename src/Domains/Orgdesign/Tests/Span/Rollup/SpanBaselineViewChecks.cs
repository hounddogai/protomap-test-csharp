namespace Acme.Workforce.Orgdesign.Tests.Span.Rollup;

using Acme.Workforce.Orgdesign.Span.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SpanBaselineViewChecks
{
    public static void SpanBaselineViewShowsTheStatusInCapitals()
    {
        var item = SpanBaseline.NewSpanBaseline("code-461", "name-42");
        var view = SpanBaselineView.ToSpanBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SpanBaselineView.ToSpanBaselineViews(new List<SpanBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SpanBaselineViewShowsTheStatusInCapitals();
    }
}
