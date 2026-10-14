namespace Acme.Workforce.Orgdesign.Tests.Span.Ledger;

using Acme.Workforce.Orgdesign.Span.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class SpanTierViewChecks
{
    public static void SpanTierViewShowsTheStatusInCapitals()
    {
        var item = SpanTier.NewSpanTier("code-792", "name-347");
        var view = SpanTierView.ToSpanTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SpanTierView.ToSpanTierViews(new List<SpanTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SpanTierViewShowsTheStatusInCapitals();
    }
}
