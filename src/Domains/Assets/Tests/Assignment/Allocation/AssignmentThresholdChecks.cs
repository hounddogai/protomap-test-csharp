namespace Acme.Workforce.Assets.Tests.Assignment.Allocation;

using Acme.Workforce.Assets.Assignment.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class AssignmentThresholdChecks
{
    public static void AssignmentThresholdStartsInProposed()
    {
        var item = AssignmentThreshold.NewAssignmentThreshold("code-684", "label-90");
        Check.Equal(item.Status, AssignmentThresholdStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssignmentThresholdMovesToRejected()
    {
        var item = AssignmentThreshold.NewAssignmentThreshold("code-684", "label-90");
        try
        {
            item.MoveTo(AssignmentThresholdStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssignmentThresholdStatus.Rejected, "status");
    }

    public static void AssignmentThresholdRejectsMovesFromWithdrawn()
    {
        var item = AssignmentThreshold.NewAssignmentThreshold("code-684", "label-90");
        item.Status = AssignmentThresholdStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(AssignmentThresholdStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        AssignmentThresholdStartsInProposed();
        AssignmentThresholdMovesToRejected();
        AssignmentThresholdRejectsMovesFromWithdrawn();
    }
}
