namespace Acme.Workforce.Engagement.Tests.Feedback.Rule;

using Acme.Workforce.Engagement.Feedback.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class BatchedFeedbackBaselineChecks
{
    public static void BatchedFeedbackBaselineStartsInOpen()
    {
        var item = BatchedFeedbackBaseline.NewBatchedFeedbackBaseline("code-949", "title-887");
        Check.Equal(item.Status, BatchedFeedbackBaselineStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedFeedbackBaselineMovesToInReview()
    {
        var item = BatchedFeedbackBaseline.NewBatchedFeedbackBaseline("code-949", "title-887");
        try
        {
            item.MoveTo(BatchedFeedbackBaselineStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedFeedbackBaselineStatus.InReview, "status");
    }

    public static void BatchedFeedbackBaselineRejectsMovesFromArchived()
    {
        var item = BatchedFeedbackBaseline.NewBatchedFeedbackBaseline("code-949", "title-887");
        item.Status = BatchedFeedbackBaselineStatus.Archived;
        Check.Fails(() => item.MoveTo(BatchedFeedbackBaselineStatus.Open), "move");
    }

    public static void RunAll()
    {
        BatchedFeedbackBaselineStartsInOpen();
        BatchedFeedbackBaselineMovesToInReview();
        BatchedFeedbackBaselineRejectsMovesFromArchived();
    }
}
