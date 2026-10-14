namespace Acme.Workforce.Workplace.Tests.AccessBadge.Ledger;

using Acme.Workforce.Workplace.AccessBadge.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class FallbackAccessBadgeAssignmentChecks
{
    public static void FallbackAccessBadgeAssignmentStartsInIdle()
    {
        var item = FallbackAccessBadgeAssignment.NewFallbackAccessBadgeAssignment("code-884", "name-239");
        Check.Equal(item.Status, FallbackAccessBadgeAssignmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackAccessBadgeAssignmentMovesToReady()
    {
        var item = FallbackAccessBadgeAssignment.NewFallbackAccessBadgeAssignment("code-884", "name-239");
        try
        {
            item.MoveTo(FallbackAccessBadgeAssignmentStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackAccessBadgeAssignmentStatus.Ready, "status");
    }

    public static void FallbackAccessBadgeAssignmentRejectsMovesFromStopped()
    {
        var item = FallbackAccessBadgeAssignment.NewFallbackAccessBadgeAssignment("code-884", "name-239");
        item.Status = FallbackAccessBadgeAssignmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(FallbackAccessBadgeAssignmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        FallbackAccessBadgeAssignmentStartsInIdle();
        FallbackAccessBadgeAssignmentMovesToReady();
        FallbackAccessBadgeAssignmentRejectsMovesFromStopped();
    }
}
