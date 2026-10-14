namespace Acme.Workforce.Engagement.Tests.ActionPlan.Slot;

using Acme.Workforce.Engagement.ActionPlan.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class ActionPlanSlotChecks
{
    public static void ActionPlanSlotStartsInRequested()
    {
        var item = ActionPlanSlot.NewActionPlanSlot("code-833", "name-304");
        Check.Equal(item.Status, ActionPlanSlotStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ActionPlanSlotMovesToGranted()
    {
        var item = ActionPlanSlot.NewActionPlanSlot("code-833", "name-304");
        try
        {
            item.MoveTo(ActionPlanSlotStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ActionPlanSlotStatus.Granted, "status");
    }

    public static void ActionPlanSlotRejectsMovesFromExpired()
    {
        var item = ActionPlanSlot.NewActionPlanSlot("code-833", "name-304");
        item.Status = ActionPlanSlotStatus.Expired;
        Check.Fails(() => item.MoveTo(ActionPlanSlotStatus.Requested), "move");
    }

    public static void RunAll()
    {
        ActionPlanSlotStartsInRequested();
        ActionPlanSlotMovesToGranted();
        ActionPlanSlotRejectsMovesFromExpired();
    }
}
