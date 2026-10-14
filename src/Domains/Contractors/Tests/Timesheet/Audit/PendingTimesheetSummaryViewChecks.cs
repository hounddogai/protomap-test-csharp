namespace Acme.Workforce.Contractors.Tests.Timesheet.Audit;

using Acme.Workforce.Contractors.Timesheet.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class PendingTimesheetSummaryViewChecks
{
    public static void PendingTimesheetSummaryViewShowsTheStatusInCapitals()
    {
        var item = PendingTimesheetSummary.NewPendingTimesheetSummary("code-943", "title-847");
        var view = PendingTimesheetSummaryView.ToPendingTimesheetSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingTimesheetSummaryView.ToPendingTimesheetSummaryViews(new List<PendingTimesheetSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingTimesheetSummaryViewShowsTheStatusInCapitals();
    }
}
