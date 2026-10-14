namespace Acme.Workforce.Engagement.Tests.Feedback.Audit;

using Acme.Workforce.Engagement.Feedback.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class FeedbackClaimViewChecks
{
    public static void FeedbackClaimViewShowsTheStatusInCapitals()
    {
        var item = FeedbackClaim.NewFeedbackClaim("code-636", "name-516");
        var view = FeedbackClaimView.ToFeedbackClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FeedbackClaimView.ToFeedbackClaimViews(new List<FeedbackClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FeedbackClaimViewShowsTheStatusInCapitals();
    }
}
