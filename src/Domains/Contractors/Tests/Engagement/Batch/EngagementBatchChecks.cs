namespace Acme.Workforce.Contractors.Tests.Engagement.Batch;

using Acme.Workforce.Contractors.Engagement.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class EngagementBatchChecks
{
    public static void EngagementBatchStartsInProposed()
    {
        var item = EngagementBatch.NewEngagementBatch("code-368", "label-488");
        Check.Equal(item.Status, EngagementBatchStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EngagementBatchMovesToApproved()
    {
        var item = EngagementBatch.NewEngagementBatch("code-368", "label-488");
        try
        {
            item.MoveTo(EngagementBatchStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EngagementBatchStatus.Approved, "status");
    }

    public static void EngagementBatchRejectsMovesFromWithdrawn()
    {
        var item = EngagementBatch.NewEngagementBatch("code-368", "label-488");
        item.Status = EngagementBatchStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(EngagementBatchStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        EngagementBatchStartsInProposed();
        EngagementBatchMovesToApproved();
        EngagementBatchRejectsMovesFromWithdrawn();
    }
}
