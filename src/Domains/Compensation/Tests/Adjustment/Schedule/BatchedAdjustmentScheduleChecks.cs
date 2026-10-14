namespace Acme.Workforce.Compensation.Tests.Adjustment.Schedule;

using Acme.Workforce.Compensation.Adjustment.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class BatchedAdjustmentScheduleChecks
{
    public static void BatchedAdjustmentScheduleStartsInNew()
    {
        var item = BatchedAdjustmentSchedule.NewBatchedAdjustmentSchedule("code-937", "label-695");
        Check.Equal(item.Status, BatchedAdjustmentScheduleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedAdjustmentScheduleMovesToDone()
    {
        var item = BatchedAdjustmentSchedule.NewBatchedAdjustmentSchedule("code-937", "label-695");
        try
        {
            item.MoveTo(BatchedAdjustmentScheduleStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedAdjustmentScheduleStatus.Done, "status");
    }

    public static void BatchedAdjustmentScheduleRejectsMovesFromFailed()
    {
        var item = BatchedAdjustmentSchedule.NewBatchedAdjustmentSchedule("code-937", "label-695");
        item.Status = BatchedAdjustmentScheduleStatus.Failed;
        Check.Fails(() => item.MoveTo(BatchedAdjustmentScheduleStatus.New), "move");
    }

    public static void RunAll()
    {
        BatchedAdjustmentScheduleStartsInNew();
        BatchedAdjustmentScheduleMovesToDone();
        BatchedAdjustmentScheduleRejectsMovesFromFailed();
    }
}
