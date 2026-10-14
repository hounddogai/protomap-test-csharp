namespace Acme.Workforce.Contractors.Tests.Timesheet.TimesheetProfile;

using Acme.Workforce.Contractors.Timesheet.TimesheetProfile;

/// <summary>Checks of timesheet profile code, run by the project's runner.</summary>
public static class TimesheetSnapshotViewChecks
{
    public static void TimesheetSnapshotViewShowsTheStatusInCapitals()
    {
        var item = TimesheetSnapshot.NewTimesheetSnapshot("code-197", "title-860");
        var view = TimesheetSnapshotView.ToTimesheetSnapshotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TimesheetSnapshotView.ToTimesheetSnapshotViews(new List<TimesheetSnapshot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TimesheetSnapshotViewShowsTheStatusInCapitals();
    }
}
