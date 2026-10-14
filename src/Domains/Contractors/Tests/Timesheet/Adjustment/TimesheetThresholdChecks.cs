namespace Acme.Workforce.Contractors.Tests.Timesheet.Adjustment;

using Acme.Workforce.Contractors.Timesheet.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class TimesheetThresholdChecks
{
    public static void TimesheetThresholdStartsInNew()
    {
        var item = TimesheetThreshold.NewTimesheetThreshold("code-20", "title-196");
        Check.Equal(item.Status, TimesheetThresholdStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TimesheetThresholdMovesToQueued()
    {
        var item = TimesheetThreshold.NewTimesheetThreshold("code-20", "title-196");
        try
        {
            item.MoveTo(TimesheetThresholdStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TimesheetThresholdStatus.Queued, "status");
    }

    public static void TimesheetThresholdRejectsMovesFromFailed()
    {
        var item = TimesheetThreshold.NewTimesheetThreshold("code-20", "title-196");
        item.Status = TimesheetThresholdStatus.Failed;
        Check.Fails(() => item.MoveTo(TimesheetThresholdStatus.New), "move");
    }

    public static void RunAll()
    {
        TimesheetThresholdStartsInNew();
        TimesheetThresholdMovesToQueued();
        TimesheetThresholdRejectsMovesFromFailed();
    }
}
