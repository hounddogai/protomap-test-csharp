namespace Acme.Workforce.Engagement.Tests.ActionPlan.Batch;

using Acme.Workforce.Engagement.ActionPlan.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class AdaptiveActionPlanSlotChecks
{
    public static void AdaptiveActionPlanSlotStartsInNew()
    {
        var item = AdaptiveActionPlanSlot.NewAdaptiveActionPlanSlot("code-487", "title-215");
        Check.Equal(item.Status, AdaptiveActionPlanSlotStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveActionPlanSlotMovesToDone()
    {
        var item = AdaptiveActionPlanSlot.NewAdaptiveActionPlanSlot("code-487", "title-215");
        try
        {
            item.MoveTo(AdaptiveActionPlanSlotStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveActionPlanSlotStatus.Done, "status");
    }

    public static void AdaptiveActionPlanSlotRejectsMovesFromFailed()
    {
        var item = AdaptiveActionPlanSlot.NewAdaptiveActionPlanSlot("code-487", "title-215");
        item.Status = AdaptiveActionPlanSlotStatus.Failed;
        Check.Fails(() => item.MoveTo(AdaptiveActionPlanSlotStatus.New), "move");
    }

    public static void RunAll()
    {
        AdaptiveActionPlanSlotStartsInNew();
        AdaptiveActionPlanSlotMovesToDone();
        AdaptiveActionPlanSlotRejectsMovesFromFailed();
    }
}
