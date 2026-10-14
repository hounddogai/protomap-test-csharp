namespace Acme.Workforce.Talent.Tests.Nomination.Milestone;

using Acme.Workforce.Talent.Nomination.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class GlobalNominationMilestoneChecks
{
    public static void GlobalNominationMilestoneStartsInIdle()
    {
        var item = GlobalNominationMilestone.NewGlobalNominationMilestone("code-629", "label-807");
        Check.Equal(item.Status, GlobalNominationMilestoneStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalNominationMilestoneMovesToReady()
    {
        var item = GlobalNominationMilestone.NewGlobalNominationMilestone("code-629", "label-807");
        try
        {
            item.MoveTo(GlobalNominationMilestoneStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalNominationMilestoneStatus.Ready, "status");
    }

    public static void GlobalNominationMilestoneRejectsMovesFromStopped()
    {
        var item = GlobalNominationMilestone.NewGlobalNominationMilestone("code-629", "label-807");
        item.Status = GlobalNominationMilestoneStatus.Stopped;
        Check.Fails(() => item.MoveTo(GlobalNominationMilestoneStatus.Idle), "move");
    }

    public static void RunAll()
    {
        GlobalNominationMilestoneStartsInIdle();
        GlobalNominationMilestoneMovesToReady();
        GlobalNominationMilestoneRejectsMovesFromStopped();
    }
}
