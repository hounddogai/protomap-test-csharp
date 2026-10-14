namespace Acme.Workforce.Compensation.Tests.Adjustment.Schedule;

using Acme.Workforce.Compensation.Adjustment.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class BatchedAdjustmentScheduleStoreChecks
{
    public static void MemoryBatchedAdjustmentScheduleStoreFindsWhatItSaved()
    {
        var store = BatchedAdjustmentScheduleStore.NewMemoryBatchedAdjustmentScheduleStore();
        var item = BatchedAdjustmentSchedule.NewBatchedAdjustmentSchedule("code-937", "label-695");
        store.Save(item);
        if (store.Find(item.Code) is not { } found)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(found.Code, item.Code, "code");
        Check.Equal(store.List().Count, 1, "count");
    }

    public static void RunAll()
    {
        MemoryBatchedAdjustmentScheduleStoreFindsWhatItSaved();
    }
}
