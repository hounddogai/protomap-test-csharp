namespace Acme.Workforce.Engagement.Tests.Feedback.Quota;

using Acme.Workforce.Engagement.Feedback.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class FeedbackNoticeViewChecks
{
    public static void FeedbackNoticeViewShowsTheStatusInCapitals()
    {
        var item = FeedbackNotice.NewFeedbackNotice("code-465", "label-478");
        var view = FeedbackNoticeView.ToFeedbackNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FeedbackNoticeView.ToFeedbackNoticeViews(new List<FeedbackNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FeedbackNoticeViewShowsTheStatusInCapitals();
    }
}
