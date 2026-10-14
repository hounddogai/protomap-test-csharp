namespace Acme.Workforce.Absence.Tests.Holiday.BatchedHolidaySchedule;

using Acme.Workforce.Absence.Holiday.BatchedHolidaySchedule;

/// <summary>Checks of batched holiday schedule code, run by the project's runner.</summary>
public static class BatchedHolidayScheduleViewChecks
{
    public static void BatchedHolidayScheduleViewShowsTheStatusInCapitals()
    {
        var item = BatchedHolidaySchedule.NewBatchedHolidaySchedule("code-1", "name-74");
        var view = BatchedHolidayScheduleView.ToBatchedHolidayScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedHolidayScheduleView.ToBatchedHolidayScheduleViews(new List<BatchedHolidaySchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedHolidayScheduleViewShowsTheStatusInCapitals();
    }
}
