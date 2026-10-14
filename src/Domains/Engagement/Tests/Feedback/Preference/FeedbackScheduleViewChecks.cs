namespace Acme.Workforce.Engagement.Tests.Feedback.Preference;

using Acme.Workforce.Engagement.Feedback.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class FeedbackScheduleViewChecks
{
    public static void FeedbackScheduleViewShowsTheStatusInCapitals()
    {
        var item = FeedbackSchedule.NewFeedbackSchedule("code-352", "label-613");
        var view = FeedbackScheduleView.ToFeedbackScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FeedbackScheduleView.ToFeedbackScheduleViews(new List<FeedbackSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FeedbackScheduleViewShowsTheStatusInCapitals();
    }
}
