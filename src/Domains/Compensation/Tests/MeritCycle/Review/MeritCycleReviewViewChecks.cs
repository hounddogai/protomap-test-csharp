namespace Acme.Workforce.Compensation.Tests.MeritCycle.Review;

using Acme.Workforce.Compensation.MeritCycle.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class MeritCycleReviewViewChecks
{
    public static void MeritCycleReviewViewShowsTheStatusInCapitals()
    {
        var item = MeritCycleReview.NewMeritCycleReview("code-139", "title-698");
        var view = MeritCycleReviewView.ToMeritCycleReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MeritCycleReviewView.ToMeritCycleReviewViews(new List<MeritCycleReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MeritCycleReviewViewShowsTheStatusInCapitals();
    }
}
