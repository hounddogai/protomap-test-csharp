namespace Acme.Workforce.Engagement.Tests.ActionPlan.Entry;

using Acme.Workforce.Engagement.ActionPlan.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class ActionPlanEntryChecks
{
    public static void ActionPlanEntryStartsInCreated()
    {
        var item = ActionPlanEntry.NewActionPlanEntry("code-498", "label-673");
        Check.Equal(item.Status, ActionPlanEntryStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ActionPlanEntryMovesToValidated()
    {
        var item = ActionPlanEntry.NewActionPlanEntry("code-498", "label-673");
        try
        {
            item.MoveTo(ActionPlanEntryStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ActionPlanEntryStatus.Validated, "status");
    }

    public static void ActionPlanEntryRejectsMovesFromRetired()
    {
        var item = ActionPlanEntry.NewActionPlanEntry("code-498", "label-673");
        item.Status = ActionPlanEntryStatus.Retired;
        Check.Fails(() => item.MoveTo(ActionPlanEntryStatus.Created), "move");
    }

    public static void RunAll()
    {
        ActionPlanEntryStartsInCreated();
        ActionPlanEntryMovesToValidated();
        ActionPlanEntryRejectsMovesFromRetired();
    }
}
