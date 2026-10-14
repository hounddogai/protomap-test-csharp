namespace Acme.Workforce.Engagement.Tests.Feedback.Score;

using Acme.Workforce.Engagement.Feedback.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class BatchedFeedbackScoreViewChecks
{
    public static void BatchedFeedbackScoreViewShowsTheStatusInCapitals()
    {
        var item = BatchedFeedbackScore.NewBatchedFeedbackScore("code-937", "title-217");
        var view = BatchedFeedbackScoreView.ToBatchedFeedbackScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedFeedbackScoreView.ToBatchedFeedbackScoreViews(new List<BatchedFeedbackScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedFeedbackScoreViewShowsTheStatusInCapitals();
    }
}
