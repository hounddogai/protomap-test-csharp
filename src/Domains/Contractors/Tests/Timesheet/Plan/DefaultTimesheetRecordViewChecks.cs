namespace Acme.Workforce.Contractors.Tests.Timesheet.Plan;

using Acme.Workforce.Contractors.Timesheet.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class DefaultTimesheetRecordViewChecks
{
    public static void DefaultTimesheetRecordViewShowsTheStatusInCapitals()
    {
        var item = DefaultTimesheetRecord.NewDefaultTimesheetRecord("code-250", "name-74");
        var view = DefaultTimesheetRecordView.ToDefaultTimesheetRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultTimesheetRecordView.ToDefaultTimesheetRecordViews(new List<DefaultTimesheetRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultTimesheetRecordViewShowsTheStatusInCapitals();
    }
}
