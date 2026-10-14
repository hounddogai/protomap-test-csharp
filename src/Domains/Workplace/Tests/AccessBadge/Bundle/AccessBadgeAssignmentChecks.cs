namespace Acme.Workforce.Workplace.Tests.AccessBadge.Bundle;

using Acme.Workforce.Workplace.AccessBadge.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class AccessBadgeAssignmentChecks
{
    public static void AccessBadgeAssignmentStartsInOpen()
    {
        var item = AccessBadgeAssignment.NewAccessBadgeAssignment("code-821", "name-904");
        Check.Equal(item.Status, AccessBadgeAssignmentStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccessBadgeAssignmentMovesToReopened()
    {
        var item = AccessBadgeAssignment.NewAccessBadgeAssignment("code-821", "name-904");
        try
        {
            item.MoveTo(AccessBadgeAssignmentStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccessBadgeAssignmentStatus.Reopened, "status");
    }

    public static void AccessBadgeAssignmentRejectsMovesFromArchived()
    {
        var item = AccessBadgeAssignment.NewAccessBadgeAssignment("code-821", "name-904");
        item.Status = AccessBadgeAssignmentStatus.Archived;
        Check.Fails(() => item.MoveTo(AccessBadgeAssignmentStatus.Open), "move");
    }

    public static void RunAll()
    {
        AccessBadgeAssignmentStartsInOpen();
        AccessBadgeAssignmentMovesToReopened();
        AccessBadgeAssignmentRejectsMovesFromArchived();
    }
}
