namespace Acme.Workforce.Contractors.Tests.Timesheet.Profile;

using Acme.Workforce.Contractors.Timesheet.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class TimesheetBatchChecks
{
    public static void TimesheetBatchStartsInCreated()
    {
        var item = TimesheetBatch.NewTimesheetBatch("code-368", "name-850");
        Check.Equal(item.Status, TimesheetBatchStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TimesheetBatchMovesToValidated()
    {
        var item = TimesheetBatch.NewTimesheetBatch("code-368", "name-850");
        try
        {
            item.MoveTo(TimesheetBatchStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TimesheetBatchStatus.Validated, "status");
    }

    public static void TimesheetBatchRejectsMovesFromRetired()
    {
        var item = TimesheetBatch.NewTimesheetBatch("code-368", "name-850");
        item.Status = TimesheetBatchStatus.Retired;
        Check.Fails(() => item.MoveTo(TimesheetBatchStatus.Created), "move");
    }

    public static void RunAll()
    {
        TimesheetBatchStartsInCreated();
        TimesheetBatchMovesToValidated();
        TimesheetBatchRejectsMovesFromRetired();
    }
}
