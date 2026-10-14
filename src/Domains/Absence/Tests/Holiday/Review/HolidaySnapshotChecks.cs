namespace Acme.Workforce.Absence.Tests.Holiday.Review;

using Acme.Workforce.Absence.Holiday.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class HolidaySnapshotChecks
{
    public static void HolidaySnapshotStartsInPlanned()
    {
        var item = HolidaySnapshot.NewHolidaySnapshot("code-131", "name-307");
        Check.Equal(item.Status, HolidaySnapshotStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HolidaySnapshotMovesToInProgress()
    {
        var item = HolidaySnapshot.NewHolidaySnapshot("code-131", "name-307");
        try
        {
            item.MoveTo(HolidaySnapshotStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HolidaySnapshotStatus.InProgress, "status");
    }

    public static void HolidaySnapshotRejectsMovesFromCancelled()
    {
        var item = HolidaySnapshot.NewHolidaySnapshot("code-131", "name-307");
        item.Status = HolidaySnapshotStatus.Cancelled;
        Check.Fails(() => item.MoveTo(HolidaySnapshotStatus.Planned), "move");
    }

    public static void RunAll()
    {
        HolidaySnapshotStartsInPlanned();
        HolidaySnapshotMovesToInProgress();
        HolidaySnapshotRejectsMovesFromCancelled();
    }
}
