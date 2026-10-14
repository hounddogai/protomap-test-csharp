namespace Acme.Workforce.Engagement.Tests.Feedback.Reservation;

using Acme.Workforce.Engagement.Feedback.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class ScheduledFeedbackRecordViewChecks
{
    public static void ScheduledFeedbackRecordViewShowsTheStatusInCapitals()
    {
        var item = ScheduledFeedbackRecord.NewScheduledFeedbackRecord("code-702", "label-142");
        var view = ScheduledFeedbackRecordView.ToScheduledFeedbackRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledFeedbackRecordView.ToScheduledFeedbackRecordViews(new List<ScheduledFeedbackRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledFeedbackRecordViewShowsTheStatusInCapitals();
    }
}
