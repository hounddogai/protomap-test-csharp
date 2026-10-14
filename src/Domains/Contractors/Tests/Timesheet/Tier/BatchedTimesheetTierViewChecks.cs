namespace Acme.Workforce.Contractors.Tests.Timesheet.Tier;

using Acme.Workforce.Contractors.Timesheet.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class BatchedTimesheetTierViewChecks
{
    public static void BatchedTimesheetTierViewShowsTheStatusInCapitals()
    {
        var item = BatchedTimesheetTier.NewBatchedTimesheetTier("code-635", "label-204");
        var view = BatchedTimesheetTierView.ToBatchedTimesheetTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedTimesheetTierView.ToBatchedTimesheetTierViews(new List<BatchedTimesheetTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedTimesheetTierViewShowsTheStatusInCapitals();
    }
}
