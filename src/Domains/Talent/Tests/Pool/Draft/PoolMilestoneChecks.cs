namespace Acme.Workforce.Talent.Tests.Pool.Draft;

using Acme.Workforce.Talent.Pool.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class PoolMilestoneChecks
{
    public static void PoolMilestoneStartsInProposed()
    {
        var item = PoolMilestone.NewPoolMilestone("code-858", "title-234");
        Check.Equal(item.Status, PoolMilestoneStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PoolMilestoneMovesToApproved()
    {
        var item = PoolMilestone.NewPoolMilestone("code-858", "title-234");
        try
        {
            item.MoveTo(PoolMilestoneStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PoolMilestoneStatus.Approved, "status");
    }

    public static void PoolMilestoneRejectsMovesFromWithdrawn()
    {
        var item = PoolMilestone.NewPoolMilestone("code-858", "title-234");
        item.Status = PoolMilestoneStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PoolMilestoneStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PoolMilestoneStartsInProposed();
        PoolMilestoneMovesToApproved();
        PoolMilestoneRejectsMovesFromWithdrawn();
    }
}
