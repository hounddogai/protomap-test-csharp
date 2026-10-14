namespace Acme.Workforce.Engagement.Tests.ActionPlan.Ledger;

using Acme.Workforce.Engagement.ActionPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class AdaptiveActionPlanMilestoneChecks
{
    public static void AdaptiveActionPlanMilestoneStartsInIdle()
    {
        var item = AdaptiveActionPlanMilestone.NewAdaptiveActionPlanMilestone("code-18", "title-133");
        Check.Equal(item.Status, AdaptiveActionPlanMilestoneStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveActionPlanMilestoneMovesToStopped()
    {
        var item = AdaptiveActionPlanMilestone.NewAdaptiveActionPlanMilestone("code-18", "title-133");
        try
        {
            item.MoveTo(AdaptiveActionPlanMilestoneStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveActionPlanMilestoneStatus.Stopped, "status");
    }

    public static void AdaptiveActionPlanMilestoneRejectsMovesFromStopped()
    {
        var item = AdaptiveActionPlanMilestone.NewAdaptiveActionPlanMilestone("code-18", "title-133");
        item.Status = AdaptiveActionPlanMilestoneStatus.Stopped;
        Check.Fails(() => item.MoveTo(AdaptiveActionPlanMilestoneStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AdaptiveActionPlanMilestoneStartsInIdle();
        AdaptiveActionPlanMilestoneMovesToStopped();
        AdaptiveActionPlanMilestoneRejectsMovesFromStopped();
    }
}
