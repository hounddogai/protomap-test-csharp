namespace Acme.Workforce.Engagement.Tests.TownHall.Milestone;

using Acme.Workforce.Engagement.TownHall.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class TownHallMilestoneChecks
{
    public static void TownHallMilestoneStartsInIdle()
    {
        var item = TownHallMilestone.NewTownHallMilestone("code-6", "label-286");
        Check.Equal(item.Status, TownHallMilestoneStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TownHallMilestoneMovesToStopped()
    {
        var item = TownHallMilestone.NewTownHallMilestone("code-6", "label-286");
        try
        {
            item.MoveTo(TownHallMilestoneStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TownHallMilestoneStatus.Stopped, "status");
    }

    public static void TownHallMilestoneRejectsMovesFromStopped()
    {
        var item = TownHallMilestone.NewTownHallMilestone("code-6", "label-286");
        item.Status = TownHallMilestoneStatus.Stopped;
        Check.Fails(() => item.MoveTo(TownHallMilestoneStatus.Idle), "move");
    }

    public static void RunAll()
    {
        TownHallMilestoneStartsInIdle();
        TownHallMilestoneMovesToStopped();
        TownHallMilestoneRejectsMovesFromStopped();
    }
}
