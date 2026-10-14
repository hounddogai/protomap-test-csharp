namespace Acme.Workforce.Contractors.Tests.Timesheet.Request;

using Acme.Workforce.Contractors.Timesheet.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class TimesheetCycleChecks
{
    public static void TimesheetCycleStartsInNew()
    {
        var item = TimesheetCycle.NewTimesheetCycle("code-668", "label-712");
        Check.Equal(item.Status, TimesheetCycleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TimesheetCycleMovesToQueued()
    {
        var item = TimesheetCycle.NewTimesheetCycle("code-668", "label-712");
        try
        {
            item.MoveTo(TimesheetCycleStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TimesheetCycleStatus.Queued, "status");
    }

    public static void TimesheetCycleRejectsMovesFromFailed()
    {
        var item = TimesheetCycle.NewTimesheetCycle("code-668", "label-712");
        item.Status = TimesheetCycleStatus.Failed;
        Check.Fails(() => item.MoveTo(TimesheetCycleStatus.New), "move");
    }

    public static void RunAll()
    {
        TimesheetCycleStartsInNew();
        TimesheetCycleMovesToQueued();
        TimesheetCycleRejectsMovesFromFailed();
    }
}
