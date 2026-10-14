namespace Acme.Workforce.Contractors.Tests.Timesheet.Estimate;

using Acme.Workforce.Contractors.Timesheet.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class LegacyTimesheetSummaryChecks
{
    public static void LegacyTimesheetSummaryStartsInNew()
    {
        var item = LegacyTimesheetSummary.NewLegacyTimesheetSummary("code-294", "label-1");
        Check.Equal(item.Status, LegacyTimesheetSummaryStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyTimesheetSummaryMovesToDone()
    {
        var item = LegacyTimesheetSummary.NewLegacyTimesheetSummary("code-294", "label-1");
        try
        {
            item.MoveTo(LegacyTimesheetSummaryStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyTimesheetSummaryStatus.Done, "status");
    }

    public static void LegacyTimesheetSummaryRejectsMovesFromFailed()
    {
        var item = LegacyTimesheetSummary.NewLegacyTimesheetSummary("code-294", "label-1");
        item.Status = LegacyTimesheetSummaryStatus.Failed;
        Check.Fails(() => item.MoveTo(LegacyTimesheetSummaryStatus.New), "move");
    }

    public static void RunAll()
    {
        LegacyTimesheetSummaryStartsInNew();
        LegacyTimesheetSummaryMovesToDone();
        LegacyTimesheetSummaryRejectsMovesFromFailed();
    }
}
