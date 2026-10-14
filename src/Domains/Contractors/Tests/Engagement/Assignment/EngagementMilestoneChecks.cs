namespace Acme.Workforce.Contractors.Tests.Engagement.Assignment;

using Acme.Workforce.Contractors.Engagement.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class EngagementMilestoneChecks
{
    public static void EngagementMilestoneStartsInProposed()
    {
        var item = EngagementMilestone.NewEngagementMilestone("code-394", "label-948");
        Check.Equal(item.Status, EngagementMilestoneStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EngagementMilestoneMovesToRejected()
    {
        var item = EngagementMilestone.NewEngagementMilestone("code-394", "label-948");
        try
        {
            item.MoveTo(EngagementMilestoneStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EngagementMilestoneStatus.Rejected, "status");
    }

    public static void EngagementMilestoneRejectsMovesFromWithdrawn()
    {
        var item = EngagementMilestone.NewEngagementMilestone("code-394", "label-948");
        item.Status = EngagementMilestoneStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(EngagementMilestoneStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        EngagementMilestoneStartsInProposed();
        EngagementMilestoneMovesToRejected();
        EngagementMilestoneRejectsMovesFromWithdrawn();
    }
}
