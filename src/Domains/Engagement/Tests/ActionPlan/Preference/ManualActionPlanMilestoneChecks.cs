namespace Acme.Workforce.Engagement.Tests.ActionPlan.Preference;

using Acme.Workforce.Engagement.ActionPlan.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ManualActionPlanMilestoneChecks
{
    public static void ManualActionPlanMilestoneStartsInIdle()
    {
        var item = ManualActionPlanMilestone.NewManualActionPlanMilestone("code-649", "label-745");
        Check.Equal(item.Status, ManualActionPlanMilestoneStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualActionPlanMilestoneMovesToReady()
    {
        var item = ManualActionPlanMilestone.NewManualActionPlanMilestone("code-649", "label-745");
        try
        {
            item.MoveTo(ManualActionPlanMilestoneStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualActionPlanMilestoneStatus.Ready, "status");
    }

    public static void ManualActionPlanMilestoneRejectsMovesFromStopped()
    {
        var item = ManualActionPlanMilestone.NewManualActionPlanMilestone("code-649", "label-745");
        item.Status = ManualActionPlanMilestoneStatus.Stopped;
        Check.Fails(() => item.MoveTo(ManualActionPlanMilestoneStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ManualActionPlanMilestoneStartsInIdle();
        ManualActionPlanMilestoneMovesToReady();
        ManualActionPlanMilestoneRejectsMovesFromStopped();
    }
}
