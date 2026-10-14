namespace Acme.Workforce.Assets.Tests.Refresh.Preference;

using Acme.Workforce.Assets.Refresh.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RefreshAssignmentChecks
{
    public static void RefreshAssignmentStartsInOpen()
    {
        var item = RefreshAssignment.NewRefreshAssignment("code-481", "label-518");
        Check.Equal(item.Status, RefreshAssignmentStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RefreshAssignmentMovesToResolved()
    {
        var item = RefreshAssignment.NewRefreshAssignment("code-481", "label-518");
        try
        {
            item.MoveTo(RefreshAssignmentStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RefreshAssignmentStatus.Resolved, "status");
    }

    public static void RefreshAssignmentRejectsMovesFromArchived()
    {
        var item = RefreshAssignment.NewRefreshAssignment("code-481", "label-518");
        item.Status = RefreshAssignmentStatus.Archived;
        Check.Fails(() => item.MoveTo(RefreshAssignmentStatus.Open), "move");
    }

    public static void RunAll()
    {
        RefreshAssignmentStartsInOpen();
        RefreshAssignmentMovesToResolved();
        RefreshAssignmentRejectsMovesFromArchived();
    }
}
