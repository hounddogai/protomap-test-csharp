namespace Acme.Workforce.Engagement.Tests.ActionPlan.Entry;

using Acme.Workforce.Engagement.ActionPlan.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class ActionPlanBatchChecks
{
    public static void ActionPlanBatchStartsInCreated()
    {
        var item = ActionPlanBatch.NewActionPlanBatch("code-149", "label-450");
        Check.Equal(item.Status, ActionPlanBatchStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ActionPlanBatchMovesToValidated()
    {
        var item = ActionPlanBatch.NewActionPlanBatch("code-149", "label-450");
        try
        {
            item.MoveTo(ActionPlanBatchStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ActionPlanBatchStatus.Validated, "status");
    }

    public static void ActionPlanBatchRejectsMovesFromRetired()
    {
        var item = ActionPlanBatch.NewActionPlanBatch("code-149", "label-450");
        item.Status = ActionPlanBatchStatus.Retired;
        Check.Fails(() => item.MoveTo(ActionPlanBatchStatus.Created), "move");
    }

    public static void RunAll()
    {
        ActionPlanBatchStartsInCreated();
        ActionPlanBatchMovesToValidated();
        ActionPlanBatchRejectsMovesFromRetired();
    }
}
