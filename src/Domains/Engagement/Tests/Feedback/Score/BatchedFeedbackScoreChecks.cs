namespace Acme.Workforce.Engagement.Tests.Feedback.Score;

using Acme.Workforce.Engagement.Feedback.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class BatchedFeedbackScoreChecks
{
    public static void BatchedFeedbackScoreStartsInOpen()
    {
        var item = BatchedFeedbackScore.NewBatchedFeedbackScore("code-937", "title-217");
        Check.Equal(item.Status, BatchedFeedbackScoreStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedFeedbackScoreMovesToInReview()
    {
        var item = BatchedFeedbackScore.NewBatchedFeedbackScore("code-937", "title-217");
        try
        {
            item.MoveTo(BatchedFeedbackScoreStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedFeedbackScoreStatus.InReview, "status");
    }

    public static void BatchedFeedbackScoreRejectsMovesFromArchived()
    {
        var item = BatchedFeedbackScore.NewBatchedFeedbackScore("code-937", "title-217");
        item.Status = BatchedFeedbackScoreStatus.Archived;
        Check.Fails(() => item.MoveTo(BatchedFeedbackScoreStatus.Open), "move");
    }

    public static void RunAll()
    {
        BatchedFeedbackScoreStartsInOpen();
        BatchedFeedbackScoreMovesToInReview();
        BatchedFeedbackScoreRejectsMovesFromArchived();
    }
}
