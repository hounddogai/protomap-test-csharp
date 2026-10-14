namespace Acme.Workforce.Orgdesign.Tests.Span.Baseline;

using Acme.Workforce.Orgdesign.Span.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class SpanQuotaViewChecks
{
    public static void SpanQuotaViewShowsTheStatusInCapitals()
    {
        var item = SpanQuota.NewSpanQuota("code-632", "label-861");
        var view = SpanQuotaView.ToSpanQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SpanQuotaView.ToSpanQuotaViews(new List<SpanQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SpanQuotaViewShowsTheStatusInCapitals();
    }
}
