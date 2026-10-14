namespace Acme.Workforce.Absence.Tests.Accrual.Checkpoint;

using Acme.Workforce.Absence.Accrual.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class ManualAccrualCheckpointChecks
{
    public static void ManualAccrualCheckpointStartsInRequested()
    {
        var item = ManualAccrualCheckpoint.NewManualAccrualCheckpoint("code-11", "title-960");
        Check.Equal(item.Status, ManualAccrualCheckpointStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualAccrualCheckpointMovesToGranted()
    {
        var item = ManualAccrualCheckpoint.NewManualAccrualCheckpoint("code-11", "title-960");
        try
        {
            item.MoveTo(ManualAccrualCheckpointStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualAccrualCheckpointStatus.Granted, "status");
    }

    public static void ManualAccrualCheckpointRejectsMovesFromExpired()
    {
        var item = ManualAccrualCheckpoint.NewManualAccrualCheckpoint("code-11", "title-960");
        item.Status = ManualAccrualCheckpointStatus.Expired;
        Check.Fails(() => item.MoveTo(ManualAccrualCheckpointStatus.Requested), "move");
    }

    public static void RunAll()
    {
        ManualAccrualCheckpointStartsInRequested();
        ManualAccrualCheckpointMovesToGranted();
        ManualAccrualCheckpointRejectsMovesFromExpired();
    }
}
