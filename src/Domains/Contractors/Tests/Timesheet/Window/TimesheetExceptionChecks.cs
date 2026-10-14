namespace Acme.Workforce.Contractors.Tests.Timesheet.Window;

using Acme.Workforce.Contractors.Timesheet.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class TimesheetExceptionChecks
{
    public static void TimesheetExceptionStartsInPlanned()
    {
        var item = TimesheetException.NewTimesheetException("code-160", "title-260");
        Check.Equal(item.Status, TimesheetExceptionStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TimesheetExceptionMovesToScheduled()
    {
        var item = TimesheetException.NewTimesheetException("code-160", "title-260");
        try
        {
            item.MoveTo(TimesheetExceptionStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TimesheetExceptionStatus.Scheduled, "status");
    }

    public static void TimesheetExceptionRejectsMovesFromCancelled()
    {
        var item = TimesheetException.NewTimesheetException("code-160", "title-260");
        item.Status = TimesheetExceptionStatus.Cancelled;
        Check.Fails(() => item.MoveTo(TimesheetExceptionStatus.Planned), "move");
    }

    public static void RunAll()
    {
        TimesheetExceptionStartsInPlanned();
        TimesheetExceptionMovesToScheduled();
        TimesheetExceptionRejectsMovesFromCancelled();
    }
}
