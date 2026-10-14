namespace Acme.Workforce.Contractors.Tests.Timesheet.TimesheetProfile;

using Acme.Workforce.Contractors.Timesheet.TimesheetProfile;

/// <summary>Checks of timesheet profile code, run by the project's runner.</summary>
public static class TimesheetDigestViewChecks
{
    public static void TimesheetDigestViewShowsTheStatusInCapitals()
    {
        var item = TimesheetDigest.NewTimesheetDigest("code-894", "label-86");
        var view = TimesheetDigestView.ToTimesheetDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TimesheetDigestView.ToTimesheetDigestViews(new List<TimesheetDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TimesheetDigestViewShowsTheStatusInCapitals();
    }
}
