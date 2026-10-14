namespace Acme.Workforce.Contractors.Tests.Timesheet.TimesheetProfile;

using Acme.Workforce.Contractors.Timesheet.TimesheetProfile;

/// <summary>Checks of timesheet profile code, run by the project's runner.</summary>
public static class TimesheetGroupChecks
{
    public static void TimesheetGroupStartsInRequested()
    {
        var item = TimesheetGroup.NewTimesheetGroup("code-131", "title-654");
        Check.Equal(item.Status, TimesheetGroupStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TimesheetGroupMovesToRevoked()
    {
        var item = TimesheetGroup.NewTimesheetGroup("code-131", "title-654");
        try
        {
            item.MoveTo(TimesheetGroupStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TimesheetGroupStatus.Revoked, "status");
    }

    public static void TimesheetGroupRejectsMovesFromExpired()
    {
        var item = TimesheetGroup.NewTimesheetGroup("code-131", "title-654");
        item.Status = TimesheetGroupStatus.Expired;
        Check.Fails(() => item.MoveTo(TimesheetGroupStatus.Requested), "move");
    }

    public static void RunAll()
    {
        TimesheetGroupStartsInRequested();
        TimesheetGroupMovesToRevoked();
        TimesheetGroupRejectsMovesFromExpired();
    }
}
