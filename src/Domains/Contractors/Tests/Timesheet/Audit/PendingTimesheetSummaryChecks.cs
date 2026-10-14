namespace Acme.Workforce.Contractors.Tests.Timesheet.Audit;

using Acme.Workforce.Contractors.Timesheet.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class PendingTimesheetSummaryChecks
{
    public static void PendingTimesheetSummaryStartsInIdle()
    {
        var item = PendingTimesheetSummary.NewPendingTimesheetSummary("code-943", "title-847");
        Check.Equal(item.Status, PendingTimesheetSummaryStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingTimesheetSummaryMovesToDraining()
    {
        var item = PendingTimesheetSummary.NewPendingTimesheetSummary("code-943", "title-847");
        try
        {
            item.MoveTo(PendingTimesheetSummaryStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingTimesheetSummaryStatus.Draining, "status");
    }

    public static void PendingTimesheetSummaryRejectsMovesFromStopped()
    {
        var item = PendingTimesheetSummary.NewPendingTimesheetSummary("code-943", "title-847");
        item.Status = PendingTimesheetSummaryStatus.Stopped;
        Check.Fails(() => item.MoveTo(PendingTimesheetSummaryStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PendingTimesheetSummaryStartsInIdle();
        PendingTimesheetSummaryMovesToDraining();
        PendingTimesheetSummaryRejectsMovesFromStopped();
    }
}
