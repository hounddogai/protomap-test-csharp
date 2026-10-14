namespace Acme.Workforce.Orgdesign.Tests.Span.TieredSpanDraft;

using Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

/// <summary>Checks of tiered span draft code, run by the project's runner.</summary>
public static class SpanEstimateViewChecks
{
    public static void SpanEstimateViewShowsTheStatusInCapitals()
    {
        var item = SpanEstimate.NewSpanEstimate("code-978", "name-708");
        var view = SpanEstimateView.ToSpanEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SpanEstimateView.ToSpanEstimateViews(new List<SpanEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SpanEstimateViewShowsTheStatusInCapitals();
    }
}
