namespace Acme.Workforce.Absence.Tests.LeaveRequest.Checkpoint;

using Acme.Workforce.Absence.LeaveRequest.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class LeaveRequestCheckpointChecks
{
    public static void LeaveRequestCheckpointStartsInNew()
    {
        var item = LeaveRequestCheckpoint.NewLeaveRequestCheckpoint("code-191", "title-367");
        Check.Equal(item.Status, LeaveRequestCheckpointStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LeaveRequestCheckpointMovesToRunning()
    {
        var item = LeaveRequestCheckpoint.NewLeaveRequestCheckpoint("code-191", "title-367");
        try
        {
            item.MoveTo(LeaveRequestCheckpointStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LeaveRequestCheckpointStatus.Running, "status");
    }

    public static void LeaveRequestCheckpointRejectsMovesFromFailed()
    {
        var item = LeaveRequestCheckpoint.NewLeaveRequestCheckpoint("code-191", "title-367");
        item.Status = LeaveRequestCheckpointStatus.Failed;
        Check.Fails(() => item.MoveTo(LeaveRequestCheckpointStatus.New), "move");
    }

    public static void RunAll()
    {
        LeaveRequestCheckpointStartsInNew();
        LeaveRequestCheckpointMovesToRunning();
        LeaveRequestCheckpointRejectsMovesFromFailed();
    }
}
