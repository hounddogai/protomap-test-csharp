namespace Acme.Workforce.Orgdesign.Tests.Span.Review;

using Acme.Workforce.Orgdesign.Span.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class StrictSpanClaimViewChecks
{
    public static void StrictSpanClaimViewShowsTheStatusInCapitals()
    {
        var item = StrictSpanClaim.NewStrictSpanClaim("code-45", "name-799");
        var view = StrictSpanClaimView.ToStrictSpanClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictSpanClaimView.ToStrictSpanClaimViews(new List<StrictSpanClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictSpanClaimViewShowsTheStatusInCapitals();
    }
}
