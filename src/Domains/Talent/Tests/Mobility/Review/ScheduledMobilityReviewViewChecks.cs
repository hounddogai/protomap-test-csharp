namespace Acme.Workforce.Talent.Tests.Mobility.Review;

using Acme.Workforce.Talent.Mobility.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class ScheduledMobilityReviewViewChecks
{
    public static void ScheduledMobilityReviewViewShowsTheStatusInCapitals()
    {
        var item = ScheduledMobilityReview.NewScheduledMobilityReview("code-415", "title-547");
        var view = ScheduledMobilityReviewView.ToScheduledMobilityReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledMobilityReviewView.ToScheduledMobilityReviewViews(new List<ScheduledMobilityReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledMobilityReviewViewShowsTheStatusInCapitals();
    }
}
