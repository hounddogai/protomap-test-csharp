namespace Acme.Workforce.Contractors.Tests.Timesheet.Preference;

using Acme.Workforce.Contractors.Timesheet.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class TimesheetWindowChecks
{
    public static void TimesheetWindowStartsInOpen()
    {
        var item = TimesheetWindow.NewTimesheetWindow("code-999", "title-394");
        Check.Equal(item.Status, TimesheetWindowStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TimesheetWindowMovesToResolved()
    {
        var item = TimesheetWindow.NewTimesheetWindow("code-999", "title-394");
        try
        {
            item.MoveTo(TimesheetWindowStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TimesheetWindowStatus.Resolved, "status");
    }

    public static void TimesheetWindowRejectsMovesFromArchived()
    {
        var item = TimesheetWindow.NewTimesheetWindow("code-999", "title-394");
        item.Status = TimesheetWindowStatus.Archived;
        Check.Fails(() => item.MoveTo(TimesheetWindowStatus.Open), "move");
    }

    public static void RunAll()
    {
        TimesheetWindowStartsInOpen();
        TimesheetWindowMovesToResolved();
        TimesheetWindowRejectsMovesFromArchived();
    }
}
