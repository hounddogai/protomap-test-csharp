namespace Acme.Workforce.Compensation.Tests.BonusPool.Variance;

using Acme.Workforce.Compensation.BonusPool.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class BatchedBonusPoolReviewChecks
{
    public static void BatchedBonusPoolReviewStartsInPlanned()
    {
        var item = BatchedBonusPoolReview.NewBatchedBonusPoolReview("code-112", "title-74");
        Check.Equal(item.Status, BatchedBonusPoolReviewStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedBonusPoolReviewMovesToScheduled()
    {
        var item = BatchedBonusPoolReview.NewBatchedBonusPoolReview("code-112", "title-74");
        try
        {
            item.MoveTo(BatchedBonusPoolReviewStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedBonusPoolReviewStatus.Scheduled, "status");
    }

    public static void BatchedBonusPoolReviewRejectsMovesFromCancelled()
    {
        var item = BatchedBonusPoolReview.NewBatchedBonusPoolReview("code-112", "title-74");
        item.Status = BatchedBonusPoolReviewStatus.Cancelled;
        Check.Fails(() => item.MoveTo(BatchedBonusPoolReviewStatus.Planned), "move");
    }

    public static void RunAll()
    {
        BatchedBonusPoolReviewStartsInPlanned();
        BatchedBonusPoolReviewMovesToScheduled();
        BatchedBonusPoolReviewRejectsMovesFromCancelled();
    }
}
