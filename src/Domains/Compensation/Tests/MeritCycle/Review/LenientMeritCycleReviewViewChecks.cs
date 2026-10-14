namespace Acme.Workforce.Compensation.Tests.MeritCycle.Review;

using Acme.Workforce.Compensation.MeritCycle.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class LenientMeritCycleReviewViewChecks
{
    public static void LenientMeritCycleReviewViewShowsTheStatusInCapitals()
    {
        var item = LenientMeritCycleReview.NewLenientMeritCycleReview("code-408", "title-730");
        var view = LenientMeritCycleReviewView.ToLenientMeritCycleReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientMeritCycleReviewView.ToLenientMeritCycleReviewViews(new List<LenientMeritCycleReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientMeritCycleReviewViewShowsTheStatusInCapitals();
    }
}
