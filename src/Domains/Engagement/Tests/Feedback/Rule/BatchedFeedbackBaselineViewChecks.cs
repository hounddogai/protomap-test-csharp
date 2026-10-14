namespace Acme.Workforce.Engagement.Tests.Feedback.Rule;

using Acme.Workforce.Engagement.Feedback.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class BatchedFeedbackBaselineViewChecks
{
    public static void BatchedFeedbackBaselineViewShowsTheStatusInCapitals()
    {
        var item = BatchedFeedbackBaseline.NewBatchedFeedbackBaseline("code-949", "title-887");
        var view = BatchedFeedbackBaselineView.ToBatchedFeedbackBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedFeedbackBaselineView.ToBatchedFeedbackBaselineViews(new List<BatchedFeedbackBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedFeedbackBaselineViewShowsTheStatusInCapitals();
    }
}
