namespace Acme.Workforce.Engagement.Tests.ActionPlan.Preference;

using Acme.Workforce.Engagement.ActionPlan.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ActionPlanPreferenceChecks
{
    public static void ActionPlanPreferenceStartsInProposed()
    {
        var item = ActionPlanPreference.NewActionPlanPreference("code-296", "name-396");
        Check.Equal(item.Status, ActionPlanPreferenceStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ActionPlanPreferenceMovesToApproved()
    {
        var item = ActionPlanPreference.NewActionPlanPreference("code-296", "name-396");
        try
        {
            item.MoveTo(ActionPlanPreferenceStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ActionPlanPreferenceStatus.Approved, "status");
    }

    public static void ActionPlanPreferenceRejectsMovesFromWithdrawn()
    {
        var item = ActionPlanPreference.NewActionPlanPreference("code-296", "name-396");
        item.Status = ActionPlanPreferenceStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ActionPlanPreferenceStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ActionPlanPreferenceStartsInProposed();
        ActionPlanPreferenceMovesToApproved();
        ActionPlanPreferenceRejectsMovesFromWithdrawn();
    }
}
