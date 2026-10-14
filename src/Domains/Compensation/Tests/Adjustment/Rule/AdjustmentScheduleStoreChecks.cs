namespace Acme.Workforce.Compensation.Tests.Adjustment.Rule;

using Acme.Workforce.Compensation.Adjustment.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AdjustmentScheduleStoreChecks
{
    public static void MemoryAdjustmentScheduleStoreFindsWhatItSaved()
    {
        var store = AdjustmentScheduleStore.NewMemoryAdjustmentScheduleStore();
        var item = AdjustmentSchedule.NewAdjustmentSchedule("code-958", "label-869");
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
        MemoryAdjustmentScheduleStoreFindsWhatItSaved();
    }
}
