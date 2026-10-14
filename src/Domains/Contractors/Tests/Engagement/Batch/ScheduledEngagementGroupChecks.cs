namespace Acme.Workforce.Contractors.Tests.Engagement.Batch;

using Acme.Workforce.Contractors.Engagement.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class ScheduledEngagementGroupChecks
{
    public static void ScheduledEngagementGroupStartsInIdle()
    {
        var item = ScheduledEngagementGroup.NewScheduledEngagementGroup("code-562", "name-284");
        Check.Equal(item.Status, ScheduledEngagementGroupStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledEngagementGroupMovesToDraining()
    {
        var item = ScheduledEngagementGroup.NewScheduledEngagementGroup("code-562", "name-284");
        try
        {
            item.MoveTo(ScheduledEngagementGroupStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledEngagementGroupStatus.Draining, "status");
    }

    public static void ScheduledEngagementGroupRejectsMovesFromStopped()
    {
        var item = ScheduledEngagementGroup.NewScheduledEngagementGroup("code-562", "name-284");
        item.Status = ScheduledEngagementGroupStatus.Stopped;
        Check.Fails(() => item.MoveTo(ScheduledEngagementGroupStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ScheduledEngagementGroupStartsInIdle();
        ScheduledEngagementGroupMovesToDraining();
        ScheduledEngagementGroupRejectsMovesFromStopped();
    }
}
