namespace Acme.Workforce.Orgdesign.Tests.Span.Preference;

using Acme.Workforce.Orgdesign.Span.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class SpanReviewViewChecks
{
    public static void SpanReviewViewShowsTheStatusInCapitals()
    {
        var item = SpanReview.NewSpanReview("code-354", "name-160");
        var view = SpanReviewView.ToSpanReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SpanReviewView.ToSpanReviewViews(new List<SpanReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SpanReviewViewShowsTheStatusInCapitals();
    }
}
