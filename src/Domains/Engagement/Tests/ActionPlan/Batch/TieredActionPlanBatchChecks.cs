namespace Acme.Workforce.Engagement.Tests.ActionPlan.Batch;

using Acme.Workforce.Engagement.ActionPlan.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class TieredActionPlanBatchChecks
{
    public static void TieredActionPlanBatchStartsInDraft()
    {
        var item = TieredActionPlanBatch.NewTieredActionPlanBatch("code-836", "label-665");
        Check.Equal(item.Status, TieredActionPlanBatchStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredActionPlanBatchMovesToClosed()
    {
        var item = TieredActionPlanBatch.NewTieredActionPlanBatch("code-836", "label-665");
        try
        {
            item.MoveTo(TieredActionPlanBatchStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredActionPlanBatchStatus.Closed, "status");
    }

    public static void TieredActionPlanBatchRejectsMovesFromClosed()
    {
        var item = TieredActionPlanBatch.NewTieredActionPlanBatch("code-836", "label-665");
        item.Status = TieredActionPlanBatchStatus.Closed;
        Check.Fails(() => item.MoveTo(TieredActionPlanBatchStatus.Draft), "move");
    }

    public static void RunAll()
    {
        TieredActionPlanBatchStartsInDraft();
        TieredActionPlanBatchMovesToClosed();
        TieredActionPlanBatchRejectsMovesFromClosed();
    }
}
