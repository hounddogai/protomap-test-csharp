namespace Acme.Workforce.Contractors.Tests.Timesheet.Policy;

using Acme.Workforce.Contractors.Timesheet.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class TimesheetDraftViewChecks
{
    public static void TimesheetDraftViewShowsTheStatusInCapitals()
    {
        var item = TimesheetDraft.NewTimesheetDraft("code-799", "label-733");
        var view = TimesheetDraftView.ToTimesheetDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TimesheetDraftView.ToTimesheetDraftViews(new List<TimesheetDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TimesheetDraftViewShowsTheStatusInCapitals();
    }
}
