namespace Acme.Workforce.Contractors.Tests.Timesheet.OverrideItem;

using Acme.Workforce.Contractors.Timesheet.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class TimesheetOverrideChecks
{
    public static void TimesheetOverrideStartsInPlanned()
    {
        var item = TimesheetOverride.NewTimesheetOverride("code-410", "label-946");
        Check.Equal(item.Status, TimesheetOverrideStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TimesheetOverrideMovesToScheduled()
    {
        var item = TimesheetOverride.NewTimesheetOverride("code-410", "label-946");
        try
        {
            item.MoveTo(TimesheetOverrideStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TimesheetOverrideStatus.Scheduled, "status");
    }

    public static void TimesheetOverrideRejectsMovesFromCancelled()
    {
        var item = TimesheetOverride.NewTimesheetOverride("code-410", "label-946");
        item.Status = TimesheetOverrideStatus.Cancelled;
        Check.Fails(() => item.MoveTo(TimesheetOverrideStatus.Planned), "move");
    }

    public static void RunAll()
    {
        TimesheetOverrideStartsInPlanned();
        TimesheetOverrideMovesToScheduled();
        TimesheetOverrideRejectsMovesFromCancelled();
    }
}
