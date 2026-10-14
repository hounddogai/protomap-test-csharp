namespace Acme.Workforce.Engagement.Tests.Feedback.Notice;

using Acme.Workforce.Engagement.Feedback.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class PrimaryFeedbackNoticeViewChecks
{
    public static void PrimaryFeedbackNoticeViewShowsTheStatusInCapitals()
    {
        var item = PrimaryFeedbackNotice.NewPrimaryFeedbackNotice("code-100", "title-935");
        var view = PrimaryFeedbackNoticeView.ToPrimaryFeedbackNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryFeedbackNoticeView.ToPrimaryFeedbackNoticeViews(new List<PrimaryFeedbackNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryFeedbackNoticeViewShowsTheStatusInCapitals();
    }
}
