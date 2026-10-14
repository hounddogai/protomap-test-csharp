namespace Acme.Workforce.Engagement.Tests.Feedback.Estimate;

using Acme.Workforce.Engagement.Feedback.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class FeedbackEstimateChecks
{
    public static void FeedbackEstimateStartsInProposed()
    {
        var item = FeedbackEstimate.NewFeedbackEstimate("code-695", "title-213");
        Check.Equal(item.Status, FeedbackEstimateStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FeedbackEstimateMovesToApproved()
    {
        var item = FeedbackEstimate.NewFeedbackEstimate("code-695", "title-213");
        try
        {
            item.MoveTo(FeedbackEstimateStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FeedbackEstimateStatus.Approved, "status");
    }

    public static void FeedbackEstimateRejectsMovesFromWithdrawn()
    {
        var item = FeedbackEstimate.NewFeedbackEstimate("code-695", "title-213");
        item.Status = FeedbackEstimateStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(FeedbackEstimateStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        FeedbackEstimateStartsInProposed();
        FeedbackEstimateMovesToApproved();
        FeedbackEstimateRejectsMovesFromWithdrawn();
    }
}
