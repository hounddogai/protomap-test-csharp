namespace Acme.Workforce.Talent.Tests.Pool.Milestone;

using Acme.Workforce.Talent.Pool.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class CappedPoolMilestoneChecks
{
    public static void CappedPoolMilestoneStartsInDraft()
    {
        var item = CappedPoolMilestone.NewCappedPoolMilestone("code-43", "name-364");
        Check.Equal(item.Status, CappedPoolMilestoneStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedPoolMilestoneMovesToActive()
    {
        var item = CappedPoolMilestone.NewCappedPoolMilestone("code-43", "name-364");
        try
        {
            item.MoveTo(CappedPoolMilestoneStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedPoolMilestoneStatus.Active, "status");
    }

    public static void CappedPoolMilestoneRejectsMovesFromClosed()
    {
        var item = CappedPoolMilestone.NewCappedPoolMilestone("code-43", "name-364");
        item.Status = CappedPoolMilestoneStatus.Closed;
        Check.Fails(() => item.MoveTo(CappedPoolMilestoneStatus.Draft), "move");
    }

    public static void RunAll()
    {
        CappedPoolMilestoneStartsInDraft();
        CappedPoolMilestoneMovesToActive();
        CappedPoolMilestoneRejectsMovesFromClosed();
    }
}
