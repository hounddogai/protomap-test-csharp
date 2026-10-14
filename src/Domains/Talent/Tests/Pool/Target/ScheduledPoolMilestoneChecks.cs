namespace Acme.Workforce.Talent.Tests.Pool.Target;

using Acme.Workforce.Talent.Pool.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class ScheduledPoolMilestoneChecks
{
    public static void ScheduledPoolMilestoneStartsInNew()
    {
        var item = ScheduledPoolMilestone.NewScheduledPoolMilestone("code-989", "label-204");
        Check.Equal(item.Status, ScheduledPoolMilestoneStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledPoolMilestoneMovesToRunning()
    {
        var item = ScheduledPoolMilestone.NewScheduledPoolMilestone("code-989", "label-204");
        try
        {
            item.MoveTo(ScheduledPoolMilestoneStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledPoolMilestoneStatus.Running, "status");
    }

    public static void ScheduledPoolMilestoneRejectsMovesFromFailed()
    {
        var item = ScheduledPoolMilestone.NewScheduledPoolMilestone("code-989", "label-204");
        item.Status = ScheduledPoolMilestoneStatus.Failed;
        Check.Fails(() => item.MoveTo(ScheduledPoolMilestoneStatus.New), "move");
    }

    public static void RunAll()
    {
        ScheduledPoolMilestoneStartsInNew();
        ScheduledPoolMilestoneMovesToRunning();
        ScheduledPoolMilestoneRejectsMovesFromFailed();
    }
}
