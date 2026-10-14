namespace Acme.Workforce.Compensation.Tests.MeritCycle.Budget;

using Acme.Workforce.Compensation.MeritCycle.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class BatchedMeritCycleReviewChecks
{
    public static void BatchedMeritCycleReviewStartsInProposed()
    {
        var item = BatchedMeritCycleReview.NewBatchedMeritCycleReview("code-492", "title-864");
        Check.Equal(item.Status, BatchedMeritCycleReviewStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedMeritCycleReviewMovesToApproved()
    {
        var item = BatchedMeritCycleReview.NewBatchedMeritCycleReview("code-492", "title-864");
        try
        {
            item.MoveTo(BatchedMeritCycleReviewStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedMeritCycleReviewStatus.Approved, "status");
    }

    public static void BatchedMeritCycleReviewRejectsMovesFromWithdrawn()
    {
        var item = BatchedMeritCycleReview.NewBatchedMeritCycleReview("code-492", "title-864");
        item.Status = BatchedMeritCycleReviewStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(BatchedMeritCycleReviewStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        BatchedMeritCycleReviewStartsInProposed();
        BatchedMeritCycleReviewMovesToApproved();
        BatchedMeritCycleReviewRejectsMovesFromWithdrawn();
    }
}
