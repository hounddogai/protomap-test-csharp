namespace Acme.Workforce.Contractors.Tests.Timesheet.TimesheetProfile;

using Acme.Workforce.Contractors.Timesheet.TimesheetProfile;

/// <summary>Checks of timesheet profile code, run by the project's runner.</summary>
public static class TimesheetDigestChecks
{
    public static void TimesheetDigestStartsInRequested()
    {
        var item = TimesheetDigest.NewTimesheetDigest("code-894", "label-86");
        Check.Equal(item.Status, TimesheetDigestStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TimesheetDigestMovesToExpired()
    {
        var item = TimesheetDigest.NewTimesheetDigest("code-894", "label-86");
        try
        {
            item.MoveTo(TimesheetDigestStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TimesheetDigestStatus.Expired, "status");
    }

    public static void TimesheetDigestRejectsMovesFromExpired()
    {
        var item = TimesheetDigest.NewTimesheetDigest("code-894", "label-86");
        item.Status = TimesheetDigestStatus.Expired;
        Check.Fails(() => item.MoveTo(TimesheetDigestStatus.Requested), "move");
    }

    public static void RunAll()
    {
        TimesheetDigestStartsInRequested();
        TimesheetDigestMovesToExpired();
        TimesheetDigestRejectsMovesFromExpired();
    }
}
