namespace Acme.Workforce.Compensation.Tests.Adjustment.Schedule;

using Acme.Workforce.Compensation.Adjustment.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class AdjustmentVarianceStoreChecks
{
    public static void MemoryAdjustmentVarianceStoreFindsWhatItSaved()
    {
        var store = AdjustmentVarianceStore.NewMemoryAdjustmentVarianceStore();
        var item = AdjustmentVariance.NewAdjustmentVariance("code-212", "label-625");
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
        MemoryAdjustmentVarianceStoreFindsWhatItSaved();
    }
}
