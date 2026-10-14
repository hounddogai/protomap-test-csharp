namespace Acme.Workforce.Contractors.Tests.Engagement.Rollup;

using Acme.Workforce.Contractors.Engagement.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class BatchedEngagementVarianceChecks
{
    public static void BatchedEngagementVarianceStartsInProposed()
    {
        var item = BatchedEngagementVariance.NewBatchedEngagementVariance("code-615", "label-636");
        Check.Equal(item.Status, BatchedEngagementVarianceStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedEngagementVarianceMovesToApproved()
    {
        var item = BatchedEngagementVariance.NewBatchedEngagementVariance("code-615", "label-636");
        try
        {
            item.MoveTo(BatchedEngagementVarianceStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedEngagementVarianceStatus.Approved, "status");
    }

    public static void BatchedEngagementVarianceRejectsMovesFromWithdrawn()
    {
        var item = BatchedEngagementVariance.NewBatchedEngagementVariance("code-615", "label-636");
        item.Status = BatchedEngagementVarianceStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(BatchedEngagementVarianceStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        BatchedEngagementVarianceStartsInProposed();
        BatchedEngagementVarianceMovesToApproved();
        BatchedEngagementVarianceRejectsMovesFromWithdrawn();
    }
}
