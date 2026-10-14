namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Batch;

using Acme.Workforce.Talent.SuccessionPlan.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class SuccessionPlanBatchChecks
{
    public static void SuccessionPlanBatchStartsInProposed()
    {
        var item = SuccessionPlanBatch.NewSuccessionPlanBatch("code-236", "title-560");
        Check.Equal(item.Status, SuccessionPlanBatchStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SuccessionPlanBatchMovesToApproved()
    {
        var item = SuccessionPlanBatch.NewSuccessionPlanBatch("code-236", "title-560");
        try
        {
            item.MoveTo(SuccessionPlanBatchStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SuccessionPlanBatchStatus.Approved, "status");
    }

    public static void SuccessionPlanBatchRejectsMovesFromWithdrawn()
    {
        var item = SuccessionPlanBatch.NewSuccessionPlanBatch("code-236", "title-560");
        item.Status = SuccessionPlanBatchStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(SuccessionPlanBatchStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        SuccessionPlanBatchStartsInProposed();
        SuccessionPlanBatchMovesToApproved();
        SuccessionPlanBatchRejectsMovesFromWithdrawn();
    }
}
