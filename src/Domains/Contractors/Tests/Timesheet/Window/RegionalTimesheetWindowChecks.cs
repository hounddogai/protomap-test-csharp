namespace Acme.Workforce.Contractors.Tests.Timesheet.Window;

using Acme.Workforce.Contractors.Timesheet.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class RegionalTimesheetWindowChecks
{
    public static void RegionalTimesheetWindowStartsInPlanned()
    {
        var item = RegionalTimesheetWindow.NewRegionalTimesheetWindow("code-776", "title-361");
        Check.Equal(item.Status, RegionalTimesheetWindowStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalTimesheetWindowMovesToCompleted()
    {
        var item = RegionalTimesheetWindow.NewRegionalTimesheetWindow("code-776", "title-361");
        try
        {
            item.MoveTo(RegionalTimesheetWindowStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalTimesheetWindowStatus.Completed, "status");
    }

    public static void RegionalTimesheetWindowRejectsMovesFromCancelled()
    {
        var item = RegionalTimesheetWindow.NewRegionalTimesheetWindow("code-776", "title-361");
        item.Status = RegionalTimesheetWindowStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RegionalTimesheetWindowStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RegionalTimesheetWindowStartsInPlanned();
        RegionalTimesheetWindowMovesToCompleted();
        RegionalTimesheetWindowRejectsMovesFromCancelled();
    }
}
