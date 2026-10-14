namespace Acme.Workforce.Workplace.Tests.AccessBadge.Target;

using Acme.Workforce.Workplace.AccessBadge.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class AccessBadgeMilestoneChecks
{
    public static void AccessBadgeMilestoneStartsInOpen()
    {
        var item = AccessBadgeMilestone.NewAccessBadgeMilestone("code-695", "name-239");
        Check.Equal(item.Status, AccessBadgeMilestoneStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccessBadgeMilestoneMovesToResolved()
    {
        var item = AccessBadgeMilestone.NewAccessBadgeMilestone("code-695", "name-239");
        try
        {
            item.MoveTo(AccessBadgeMilestoneStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccessBadgeMilestoneStatus.Resolved, "status");
    }

    public static void AccessBadgeMilestoneRejectsMovesFromArchived()
    {
        var item = AccessBadgeMilestone.NewAccessBadgeMilestone("code-695", "name-239");
        item.Status = AccessBadgeMilestoneStatus.Archived;
        Check.Fails(() => item.MoveTo(AccessBadgeMilestoneStatus.Open), "move");
    }

    public static void RunAll()
    {
        AccessBadgeMilestoneStartsInOpen();
        AccessBadgeMilestoneMovesToResolved();
        AccessBadgeMilestoneRejectsMovesFromArchived();
    }
}
