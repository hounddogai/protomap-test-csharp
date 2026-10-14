namespace Acme.Workforce.Orgdesign.Tests.Span.Threshold;

using Acme.Workforce.Orgdesign.Span.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class SpanThresholdViewChecks
{
    public static void SpanThresholdViewShowsTheStatusInCapitals()
    {
        var item = SpanThreshold.NewSpanThreshold("code-974", "name-532");
        var view = SpanThresholdView.ToSpanThresholdView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SpanThresholdView.ToSpanThresholdViews(new List<SpanThreshold> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SpanThresholdViewShowsTheStatusInCapitals();
    }
}
