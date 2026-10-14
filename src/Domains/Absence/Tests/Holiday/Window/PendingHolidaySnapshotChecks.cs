namespace Acme.Workforce.Absence.Tests.Holiday.Window;

using Acme.Workforce.Absence.Holiday.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class PendingHolidaySnapshotChecks
{
    public static void PendingHolidaySnapshotStartsInNew()
    {
        var item = PendingHolidaySnapshot.NewPendingHolidaySnapshot("code-378", "name-699");
        Check.Equal(item.Status, PendingHolidaySnapshotStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingHolidaySnapshotMovesToQueued()
    {
        var item = PendingHolidaySnapshot.NewPendingHolidaySnapshot("code-378", "name-699");
        try
        {
            item.MoveTo(PendingHolidaySnapshotStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingHolidaySnapshotStatus.Queued, "status");
    }

    public static void PendingHolidaySnapshotRejectsMovesFromFailed()
    {
        var item = PendingHolidaySnapshot.NewPendingHolidaySnapshot("code-378", "name-699");
        item.Status = PendingHolidaySnapshotStatus.Failed;
        Check.Fails(() => item.MoveTo(PendingHolidaySnapshotStatus.New), "move");
    }

    public static void RunAll()
    {
        PendingHolidaySnapshotStartsInNew();
        PendingHolidaySnapshotMovesToQueued();
        PendingHolidaySnapshotRejectsMovesFromFailed();
    }
}
