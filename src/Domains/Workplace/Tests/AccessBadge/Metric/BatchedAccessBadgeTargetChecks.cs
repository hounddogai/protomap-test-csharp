namespace Acme.Workforce.Workplace.Tests.AccessBadge.Metric;

using Acme.Workforce.Workplace.AccessBadge.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class BatchedAccessBadgeTargetChecks
{
    public static void BatchedAccessBadgeTargetStartsInIdle()
    {
        var item = BatchedAccessBadgeTarget.NewBatchedAccessBadgeTarget("code-10", "title-996");
        Check.Equal(item.Status, BatchedAccessBadgeTargetStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedAccessBadgeTargetMovesToDraining()
    {
        var item = BatchedAccessBadgeTarget.NewBatchedAccessBadgeTarget("code-10", "title-996");
        try
        {
            item.MoveTo(BatchedAccessBadgeTargetStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedAccessBadgeTargetStatus.Draining, "status");
    }

    public static void BatchedAccessBadgeTargetRejectsMovesFromStopped()
    {
        var item = BatchedAccessBadgeTarget.NewBatchedAccessBadgeTarget("code-10", "title-996");
        item.Status = BatchedAccessBadgeTargetStatus.Stopped;
        Check.Fails(() => item.MoveTo(BatchedAccessBadgeTargetStatus.Idle), "move");
    }

    public static void RunAll()
    {
        BatchedAccessBadgeTargetStartsInIdle();
        BatchedAccessBadgeTargetMovesToDraining();
        BatchedAccessBadgeTargetRejectsMovesFromStopped();
    }
}
