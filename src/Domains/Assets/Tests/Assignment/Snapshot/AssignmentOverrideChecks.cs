namespace Acme.Workforce.Assets.Tests.Assignment.Snapshot;

using Acme.Workforce.Assets.Assignment.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class AssignmentOverrideChecks
{
    public static void AssignmentOverrideStartsInPlanned()
    {
        var item = AssignmentOverride.NewAssignmentOverride("code-229", "label-505");
        Check.Equal(item.Status, AssignmentOverrideStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssignmentOverrideMovesToInProgress()
    {
        var item = AssignmentOverride.NewAssignmentOverride("code-229", "label-505");
        try
        {
            item.MoveTo(AssignmentOverrideStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssignmentOverrideStatus.InProgress, "status");
    }

    public static void AssignmentOverrideRejectsMovesFromCancelled()
    {
        var item = AssignmentOverride.NewAssignmentOverride("code-229", "label-505");
        item.Status = AssignmentOverrideStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AssignmentOverrideStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AssignmentOverrideStartsInPlanned();
        AssignmentOverrideMovesToInProgress();
        AssignmentOverrideRejectsMovesFromCancelled();
    }
}
