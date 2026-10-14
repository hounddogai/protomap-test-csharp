namespace Acme.Workforce.Contractors.Tests.Timesheet.Rollup;

using Acme.Workforce.Contractors.Timesheet.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class TimesheetRollupChecks
{
    public static void TimesheetRollupStartsInRequested()
    {
        var item = TimesheetRollup.NewTimesheetRollup("code-379", "label-419");
        Check.Equal(item.Status, TimesheetRollupStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TimesheetRollupMovesToRevoked()
    {
        var item = TimesheetRollup.NewTimesheetRollup("code-379", "label-419");
        try
        {
            item.MoveTo(TimesheetRollupStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TimesheetRollupStatus.Revoked, "status");
    }

    public static void TimesheetRollupRejectsMovesFromExpired()
    {
        var item = TimesheetRollup.NewTimesheetRollup("code-379", "label-419");
        item.Status = TimesheetRollupStatus.Expired;
        Check.Fails(() => item.MoveTo(TimesheetRollupStatus.Requested), "move");
    }

    public static void RunAll()
    {
        TimesheetRollupStartsInRequested();
        TimesheetRollupMovesToRevoked();
        TimesheetRollupRejectsMovesFromExpired();
    }
}
