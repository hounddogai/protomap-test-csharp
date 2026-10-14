namespace Acme.Workforce.Engagement.Tests.Feedback.Snapshot;

using Acme.Workforce.Engagement.Feedback.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class FeedbackQuotaViewChecks
{
    public static void FeedbackQuotaViewShowsTheStatusInCapitals()
    {
        var item = FeedbackQuota.NewFeedbackQuota("code-86", "name-882");
        var view = FeedbackQuotaView.ToFeedbackQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FeedbackQuotaView.ToFeedbackQuotaViews(new List<FeedbackQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FeedbackQuotaViewShowsTheStatusInCapitals();
    }
}
