namespace Acme.Workforce.Contractors.Tests.Timesheet.Batch;

using Acme.Workforce.Contractors.Timesheet.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class GlobalTimesheetBatchChecks
{
    public static void GlobalTimesheetBatchStartsInDraft()
    {
        var item = GlobalTimesheetBatch.NewGlobalTimesheetBatch("code-206", "title-339");
        Check.Equal(item.Status, GlobalTimesheetBatchStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalTimesheetBatchMovesToPending()
    {
        var item = GlobalTimesheetBatch.NewGlobalTimesheetBatch("code-206", "title-339");
        try
        {
            item.MoveTo(GlobalTimesheetBatchStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalTimesheetBatchStatus.Pending, "status");
    }

    public static void GlobalTimesheetBatchRejectsMovesFromClosed()
    {
        var item = GlobalTimesheetBatch.NewGlobalTimesheetBatch("code-206", "title-339");
        item.Status = GlobalTimesheetBatchStatus.Closed;
        Check.Fails(() => item.MoveTo(GlobalTimesheetBatchStatus.Draft), "move");
    }

    public static void RunAll()
    {
        GlobalTimesheetBatchStartsInDraft();
        GlobalTimesheetBatchMovesToPending();
        GlobalTimesheetBatchRejectsMovesFromClosed();
    }
}
