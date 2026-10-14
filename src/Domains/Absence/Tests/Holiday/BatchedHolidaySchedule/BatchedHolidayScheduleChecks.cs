namespace Acme.Workforce.Absence.Tests.Holiday.BatchedHolidaySchedule;

using Acme.Workforce.Absence.Holiday.BatchedHolidaySchedule;

/// <summary>Checks of batched holiday schedule code, run by the project's runner.</summary>
public static class BatchedHolidayScheduleChecks
{
    public static void BatchedHolidayScheduleStartsInCreated()
    {
        var item = BatchedHolidaySchedule.NewBatchedHolidaySchedule("code-1", "name-74");
        Check.Equal(item.Status, BatchedHolidayScheduleStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedHolidayScheduleMovesToPublished()
    {
        var item = BatchedHolidaySchedule.NewBatchedHolidaySchedule("code-1", "name-74");
        try
        {
            item.MoveTo(BatchedHolidayScheduleStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedHolidayScheduleStatus.Published, "status");
    }

    public static void BatchedHolidayScheduleRejectsMovesFromRetired()
    {
        var item = BatchedHolidaySchedule.NewBatchedHolidaySchedule("code-1", "name-74");
        item.Status = BatchedHolidayScheduleStatus.Retired;
        Check.Fails(() => item.MoveTo(BatchedHolidayScheduleStatus.Created), "move");
    }

    public static void RunAll()
    {
        BatchedHolidayScheduleStartsInCreated();
        BatchedHolidayScheduleMovesToPublished();
        BatchedHolidayScheduleRejectsMovesFromRetired();
    }
}
