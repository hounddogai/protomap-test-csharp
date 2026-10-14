namespace Acme.Workforce.Absence.Tests.Accrual.Batch;

using Acme.Workforce.Absence.Accrual.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class AccrualEstimateStoreChecks
{
    public static void MemoryAccrualEstimateStoreFindsWhatItSaved()
    {
        var store = AccrualEstimateStore.NewMemoryAccrualEstimateStore();
        var item = AccrualEstimate.NewAccrualEstimate("code-344", "name-374");
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
        MemoryAccrualEstimateStoreFindsWhatItSaved();
    }
}
