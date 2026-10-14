namespace Acme.Workforce.Assets.Tests.Assignment.Claim;

using Acme.Workforce.Assets.Assignment.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class ScheduledAssignmentCheckpointChecks
{
    public static void ScheduledAssignmentCheckpointStartsInNew()
    {
        var item = ScheduledAssignmentCheckpoint.NewScheduledAssignmentCheckpoint("code-3", "title-733");
        Check.Equal(item.Status, ScheduledAssignmentCheckpointStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledAssignmentCheckpointMovesToRunning()
    {
        var item = ScheduledAssignmentCheckpoint.NewScheduledAssignmentCheckpoint("code-3", "title-733");
        try
        {
            item.MoveTo(ScheduledAssignmentCheckpointStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledAssignmentCheckpointStatus.Running, "status");
    }

    public static void ScheduledAssignmentCheckpointRejectsMovesFromFailed()
    {
        var item = ScheduledAssignmentCheckpoint.NewScheduledAssignmentCheckpoint("code-3", "title-733");
        item.Status = ScheduledAssignmentCheckpointStatus.Failed;
        Check.Fails(() => item.MoveTo(ScheduledAssignmentCheckpointStatus.New), "move");
    }

    public static void RunAll()
    {
        ScheduledAssignmentCheckpointStartsInNew();
        ScheduledAssignmentCheckpointMovesToRunning();
        ScheduledAssignmentCheckpointRejectsMovesFromFailed();
    }
}
