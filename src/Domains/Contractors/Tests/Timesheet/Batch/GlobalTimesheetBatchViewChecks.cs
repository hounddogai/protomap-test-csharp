namespace Acme.Workforce.Contractors.Tests.Timesheet.Batch;

using Acme.Workforce.Contractors.Timesheet.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class GlobalTimesheetBatchViewChecks
{
    public static void GlobalTimesheetBatchViewShowsTheStatusInCapitals()
    {
        var item = GlobalTimesheetBatch.NewGlobalTimesheetBatch("code-206", "title-339");
        var view = GlobalTimesheetBatchView.ToGlobalTimesheetBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalTimesheetBatchView.ToGlobalTimesheetBatchViews(new List<GlobalTimesheetBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalTimesheetBatchViewShowsTheStatusInCapitals();
    }
}
