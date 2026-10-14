namespace Acme.Workforce.Contractors.Tests.Rate.Review;

using Acme.Workforce.Contractors.Rate.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RateReviewViewChecks
{
    public static void RateReviewViewShowsTheStatusInCapitals()
    {
        var item = RateReview.NewRateReview("code-112", "name-982");
        var view = RateReviewView.ToRateReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RateReviewView.ToRateReviewViews(new List<RateReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RateReviewViewShowsTheStatusInCapitals();
    }
}
