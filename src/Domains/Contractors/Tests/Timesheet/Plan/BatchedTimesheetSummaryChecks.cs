namespace Acme.Workforce.Contractors.Tests.Timesheet.Plan;

using Acme.Workforce.Contractors.Timesheet.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class BatchedTimesheetSummaryChecks
{
    public static void BatchedTimesheetSummaryStartsInDraft()
    {
        var item = BatchedTimesheetSummary.NewBatchedTimesheetSummary("code-745", "title-471");
        Check.Equal(item.Status, BatchedTimesheetSummaryStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedTimesheetSummaryMovesToClosed()
    {
        var item = BatchedTimesheetSummary.NewBatchedTimesheetSummary("code-745", "title-471");
        try
        {
            item.MoveTo(BatchedTimesheetSummaryStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedTimesheetSummaryStatus.Closed, "status");
    }

    public static void BatchedTimesheetSummaryRejectsMovesFromClosed()
    {
        var item = BatchedTimesheetSummary.NewBatchedTimesheetSummary("code-745", "title-471");
        item.Status = BatchedTimesheetSummaryStatus.Closed;
        Check.Fails(() => item.MoveTo(BatchedTimesheetSummaryStatus.Draft), "move");
    }

    public static void RunAll()
    {
        BatchedTimesheetSummaryStartsInDraft();
        BatchedTimesheetSummaryMovesToClosed();
        BatchedTimesheetSummaryRejectsMovesFromClosed();
    }
}
