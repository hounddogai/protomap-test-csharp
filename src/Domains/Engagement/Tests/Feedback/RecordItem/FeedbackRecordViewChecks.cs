namespace Acme.Workforce.Engagement.Tests.Feedback.RecordItem;

using Acme.Workforce.Engagement.Feedback.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class FeedbackRecordViewChecks
{
    public static void FeedbackRecordViewShowsTheStatusInCapitals()
    {
        var item = FeedbackRecord.NewFeedbackRecord("code-642", "label-271");
        var view = FeedbackRecordView.ToFeedbackRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FeedbackRecordView.ToFeedbackRecordViews(new List<FeedbackRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FeedbackRecordViewShowsTheStatusInCapitals();
    }
}
