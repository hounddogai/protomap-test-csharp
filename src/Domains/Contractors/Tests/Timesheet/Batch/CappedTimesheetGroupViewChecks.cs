namespace Acme.Workforce.Contractors.Tests.Timesheet.Batch;

using Acme.Workforce.Contractors.Timesheet.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class CappedTimesheetGroupViewChecks
{
    public static void CappedTimesheetGroupViewShowsTheStatusInCapitals()
    {
        var item = CappedTimesheetGroup.NewCappedTimesheetGroup("code-900", "name-137");
        var view = CappedTimesheetGroupView.ToCappedTimesheetGroupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedTimesheetGroupView.ToCappedTimesheetGroupViews(new List<CappedTimesheetGroup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedTimesheetGroupViewShowsTheStatusInCapitals();
    }
}
