namespace Acme.Workforce.Talent.Tests.Pool.Target;

using Acme.Workforce.Talent.Pool.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class StrictPoolMilestoneChecks
{
    public static void StrictPoolMilestoneStartsInProposed()
    {
        var item = StrictPoolMilestone.NewStrictPoolMilestone("code-516", "label-27");
        Check.Equal(item.Status, StrictPoolMilestoneStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictPoolMilestoneMovesToRejected()
    {
        var item = StrictPoolMilestone.NewStrictPoolMilestone("code-516", "label-27");
        try
        {
            item.MoveTo(StrictPoolMilestoneStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictPoolMilestoneStatus.Rejected, "status");
    }

    public static void StrictPoolMilestoneRejectsMovesFromWithdrawn()
    {
        var item = StrictPoolMilestone.NewStrictPoolMilestone("code-516", "label-27");
        item.Status = StrictPoolMilestoneStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(StrictPoolMilestoneStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        StrictPoolMilestoneStartsInProposed();
        StrictPoolMilestoneMovesToRejected();
        StrictPoolMilestoneRejectsMovesFromWithdrawn();
    }
}
