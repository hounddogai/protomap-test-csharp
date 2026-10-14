namespace Acme.Workforce.Absence.Tests.Holiday.Review;

using Acme.Workforce.Absence.Holiday.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class ScheduledHolidaySnapshotChecks
{
    public static void ScheduledHolidaySnapshotStartsInCreated()
    {
        var item = ScheduledHolidaySnapshot.NewScheduledHolidaySnapshot("code-563", "label-418");
        Check.Equal(item.Status, ScheduledHolidaySnapshotStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledHolidaySnapshotMovesToPublished()
    {
        var item = ScheduledHolidaySnapshot.NewScheduledHolidaySnapshot("code-563", "label-418");
        try
        {
            item.MoveTo(ScheduledHolidaySnapshotStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledHolidaySnapshotStatus.Published, "status");
    }

    public static void ScheduledHolidaySnapshotRejectsMovesFromRetired()
    {
        var item = ScheduledHolidaySnapshot.NewScheduledHolidaySnapshot("code-563", "label-418");
        item.Status = ScheduledHolidaySnapshotStatus.Retired;
        Check.Fails(() => item.MoveTo(ScheduledHolidaySnapshotStatus.Created), "move");
    }

    public static void RunAll()
    {
        ScheduledHolidaySnapshotStartsInCreated();
        ScheduledHolidaySnapshotMovesToPublished();
        ScheduledHolidaySnapshotRejectsMovesFromRetired();
    }
}
