namespace Acme.Workforce.Absence.Tests.Accrual.AccrualSlot;

using Acme.Workforce.Absence.Accrual.AccrualSlot;

/// <summary>Checks of accrual slot code, run by the project's runner.</summary>
public static class SeasonalAccrualBatchStoreChecks
{
    public static void MemorySeasonalAccrualBatchStoreFindsWhatItSaved()
    {
        var store = SeasonalAccrualBatchStore.NewMemorySeasonalAccrualBatchStore();
        var item = SeasonalAccrualBatch.NewSeasonalAccrualBatch("code-663", "name-791");
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
        MemorySeasonalAccrualBatchStoreFindsWhatItSaved();
    }
}
