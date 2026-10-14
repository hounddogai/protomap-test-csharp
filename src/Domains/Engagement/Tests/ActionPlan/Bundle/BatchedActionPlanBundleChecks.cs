namespace Acme.Workforce.Engagement.Tests.ActionPlan.Bundle;

using Acme.Workforce.Engagement.ActionPlan.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class BatchedActionPlanBundleChecks
{
    public static void BatchedActionPlanBundleStartsInOpen()
    {
        var item = BatchedActionPlanBundle.NewBatchedActionPlanBundle("code-432", "title-756");
        Check.Equal(item.Status, BatchedActionPlanBundleStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedActionPlanBundleMovesToInReview()
    {
        var item = BatchedActionPlanBundle.NewBatchedActionPlanBundle("code-432", "title-756");
        try
        {
            item.MoveTo(BatchedActionPlanBundleStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedActionPlanBundleStatus.InReview, "status");
    }

    public static void BatchedActionPlanBundleRejectsMovesFromArchived()
    {
        var item = BatchedActionPlanBundle.NewBatchedActionPlanBundle("code-432", "title-756");
        item.Status = BatchedActionPlanBundleStatus.Archived;
        Check.Fails(() => item.MoveTo(BatchedActionPlanBundleStatus.Open), "move");
    }

    public static void RunAll()
    {
        BatchedActionPlanBundleStartsInOpen();
        BatchedActionPlanBundleMovesToInReview();
        BatchedActionPlanBundleRejectsMovesFromArchived();
    }
}
