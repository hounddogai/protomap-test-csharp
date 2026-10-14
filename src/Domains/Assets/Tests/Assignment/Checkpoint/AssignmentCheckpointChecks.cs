namespace Acme.Workforce.Assets.Tests.Assignment.Checkpoint;

using Acme.Workforce.Assets.Assignment.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class AssignmentCheckpointChecks
{
    public static void AssignmentCheckpointStartsInNew()
    {
        var item = AssignmentCheckpoint.NewAssignmentCheckpoint("code-510", "label-98");
        Check.Equal(item.Status, AssignmentCheckpointStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssignmentCheckpointMovesToRunning()
    {
        var item = AssignmentCheckpoint.NewAssignmentCheckpoint("code-510", "label-98");
        try
        {
            item.MoveTo(AssignmentCheckpointStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssignmentCheckpointStatus.Running, "status");
    }

    public static void AssignmentCheckpointRejectsMovesFromFailed()
    {
        var item = AssignmentCheckpoint.NewAssignmentCheckpoint("code-510", "label-98");
        item.Status = AssignmentCheckpointStatus.Failed;
        Check.Fails(() => item.MoveTo(AssignmentCheckpointStatus.New), "move");
    }

    public static void RunAll()
    {
        AssignmentCheckpointStartsInNew();
        AssignmentCheckpointMovesToRunning();
        AssignmentCheckpointRejectsMovesFromFailed();
    }
}
