namespace Acme.Workforce.Compensation.Tests.Adjustment.Schedule;

using Acme.Workforce.Compensation.Adjustment.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class BatchedAdjustmentScheduleViewChecks
{
    public static void BatchedAdjustmentScheduleViewShowsTheStatusInCapitals()
    {
        var item = BatchedAdjustmentSchedule.NewBatchedAdjustmentSchedule("code-937", "label-695");
        var view = BatchedAdjustmentScheduleView.ToBatchedAdjustmentScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedAdjustmentScheduleView.ToBatchedAdjustmentScheduleViews(new List<BatchedAdjustmentSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedAdjustmentScheduleViewShowsTheStatusInCapitals();
    }
}
