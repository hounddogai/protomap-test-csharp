namespace Acme.Workforce.Contractors.Tests.Timesheet.Window;

using Acme.Workforce.Contractors.Timesheet.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class TimesheetVarianceChecks
{
    public static void TimesheetVarianceStartsInNew()
    {
        var item = TimesheetVariance.NewTimesheetVariance("code-7", "label-670");
        Check.Equal(item.Status, TimesheetVarianceStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TimesheetVarianceMovesToDone()
    {
        var item = TimesheetVariance.NewTimesheetVariance("code-7", "label-670");
        try
        {
            item.MoveTo(TimesheetVarianceStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TimesheetVarianceStatus.Done, "status");
    }

    public static void TimesheetVarianceRejectsMovesFromFailed()
    {
        var item = TimesheetVariance.NewTimesheetVariance("code-7", "label-670");
        item.Status = TimesheetVarianceStatus.Failed;
        Check.Fails(() => item.MoveTo(TimesheetVarianceStatus.New), "move");
    }

    public static void RunAll()
    {
        TimesheetVarianceStartsInNew();
        TimesheetVarianceMovesToDone();
        TimesheetVarianceRejectsMovesFromFailed();
    }
}
