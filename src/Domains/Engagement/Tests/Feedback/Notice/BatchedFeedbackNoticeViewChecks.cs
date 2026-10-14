namespace Acme.Workforce.Engagement.Tests.Feedback.Notice;

using Acme.Workforce.Engagement.Feedback.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class BatchedFeedbackNoticeViewChecks
{
    public static void BatchedFeedbackNoticeViewShowsTheStatusInCapitals()
    {
        var item = BatchedFeedbackNotice.NewBatchedFeedbackNotice("code-33", "label-412");
        var view = BatchedFeedbackNoticeView.ToBatchedFeedbackNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedFeedbackNoticeView.ToBatchedFeedbackNoticeViews(new List<BatchedFeedbackNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedFeedbackNoticeViewShowsTheStatusInCapitals();
    }
}
