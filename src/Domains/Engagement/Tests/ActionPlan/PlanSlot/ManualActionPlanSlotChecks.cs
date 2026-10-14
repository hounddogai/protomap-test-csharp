namespace Acme.Workforce.Engagement.Tests.ActionPlan.PlanSlot;

using Acme.Workforce.Engagement.ActionPlan.PlanSlot;

/// <summary>Checks of plan slot code, run by the project's runner.</summary>
public static class ManualActionPlanSlotChecks
{
    public static void ManualActionPlanSlotStartsInNew()
    {
        var item = ManualActionPlanSlot.NewManualActionPlanSlot("code-303", "name-195");
        Check.Equal(item.Status, ManualActionPlanSlotStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualActionPlanSlotMovesToFailed()
    {
        var item = ManualActionPlanSlot.NewManualActionPlanSlot("code-303", "name-195");
        try
        {
            item.MoveTo(ManualActionPlanSlotStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualActionPlanSlotStatus.Failed, "status");
    }

    public static void ManualActionPlanSlotRejectsMovesFromFailed()
    {
        var item = ManualActionPlanSlot.NewManualActionPlanSlot("code-303", "name-195");
        item.Status = ManualActionPlanSlotStatus.Failed;
        Check.Fails(() => item.MoveTo(ManualActionPlanSlotStatus.New), "move");
    }

    public static void RunAll()
    {
        ManualActionPlanSlotStartsInNew();
        ManualActionPlanSlotMovesToFailed();
        ManualActionPlanSlotRejectsMovesFromFailed();
    }
}
