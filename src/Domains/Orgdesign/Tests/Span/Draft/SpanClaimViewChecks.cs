namespace Acme.Workforce.Orgdesign.Tests.Span.Draft;

using Acme.Workforce.Orgdesign.Span.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class SpanClaimViewChecks
{
    public static void SpanClaimViewShowsTheStatusInCapitals()
    {
        var item = SpanClaim.NewSpanClaim("code-490", "name-450");
        var view = SpanClaimView.ToSpanClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SpanClaimView.ToSpanClaimViews(new List<SpanClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SpanClaimViewShowsTheStatusInCapitals();
    }
}
