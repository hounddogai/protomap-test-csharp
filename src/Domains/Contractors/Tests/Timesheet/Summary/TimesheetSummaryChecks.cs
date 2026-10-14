namespace Acme.Workforce.Contractors.Tests.Timesheet.Summary;

using Acme.Workforce.Contractors.Timesheet.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class TimesheetSummaryChecks
{
    public static void TimesheetSummaryStartsInCreated()
    {
        var item = TimesheetSummary.NewTimesheetSummary("code-541", "name-859");
        Check.Equal(item.Status, TimesheetSummaryStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TimesheetSummaryMovesToValidated()
    {
        var item = TimesheetSummary.NewTimesheetSummary("code-541", "name-859");
        try
        {
            item.MoveTo(TimesheetSummaryStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TimesheetSummaryStatus.Validated, "status");
    }

    public static void TimesheetSummaryRejectsMovesFromRetired()
    {
        var item = TimesheetSummary.NewTimesheetSummary("code-541", "name-859");
        item.Status = TimesheetSummaryStatus.Retired;
        Check.Fails(() => item.MoveTo(TimesheetSummaryStatus.Created), "move");
    }

    public static void RunAll()
    {
        TimesheetSummaryStartsInCreated();
        TimesheetSummaryMovesToValidated();
        TimesheetSummaryRejectsMovesFromRetired();
    }
}
