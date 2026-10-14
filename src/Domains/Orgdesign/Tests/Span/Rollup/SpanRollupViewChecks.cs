namespace Acme.Workforce.Orgdesign.Tests.Span.Rollup;

using Acme.Workforce.Orgdesign.Span.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SpanRollupViewChecks
{
    public static void SpanRollupViewShowsTheStatusInCapitals()
    {
        var item = SpanRollup.NewSpanRollup("code-807", "name-805");
        var view = SpanRollupView.ToSpanRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SpanRollupView.ToSpanRollupViews(new List<SpanRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SpanRollupViewShowsTheStatusInCapitals();
    }
}
