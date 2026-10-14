namespace Acme.Workforce.Engagement.Tests.Feedback.Group;

using Acme.Workforce.Engagement.Feedback.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class BatchedFeedbackBatchViewChecks
{
    public static void BatchedFeedbackBatchViewShowsTheStatusInCapitals()
    {
        var item = BatchedFeedbackBatch.NewBatchedFeedbackBatch("code-771", "title-517");
        var view = BatchedFeedbackBatchView.ToBatchedFeedbackBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedFeedbackBatchView.ToBatchedFeedbackBatchViews(new List<BatchedFeedbackBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedFeedbackBatchViewShowsTheStatusInCapitals();
    }
}
