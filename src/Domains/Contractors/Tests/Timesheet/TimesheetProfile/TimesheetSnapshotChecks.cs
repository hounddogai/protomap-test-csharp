namespace Acme.Workforce.Contractors.Tests.Timesheet.TimesheetProfile;

using Acme.Workforce.Contractors.Timesheet.TimesheetProfile;

/// <summary>Checks of timesheet profile code, run by the project's runner.</summary>
public static class TimesheetSnapshotChecks
{
    public static void TimesheetSnapshotStartsInCreated()
    {
        var item = TimesheetSnapshot.NewTimesheetSnapshot("code-197", "title-860");
        Check.Equal(item.Status, TimesheetSnapshotStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TimesheetSnapshotMovesToValidated()
    {
        var item = TimesheetSnapshot.NewTimesheetSnapshot("code-197", "title-860");
        try
        {
            item.MoveTo(TimesheetSnapshotStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TimesheetSnapshotStatus.Validated, "status");
    }

    public static void TimesheetSnapshotRejectsMovesFromRetired()
    {
        var item = TimesheetSnapshot.NewTimesheetSnapshot("code-197", "title-860");
        item.Status = TimesheetSnapshotStatus.Retired;
        Check.Fails(() => item.MoveTo(TimesheetSnapshotStatus.Created), "move");
    }

    public static void RunAll()
    {
        TimesheetSnapshotStartsInCreated();
        TimesheetSnapshotMovesToValidated();
        TimesheetSnapshotRejectsMovesFromRetired();
    }
}
