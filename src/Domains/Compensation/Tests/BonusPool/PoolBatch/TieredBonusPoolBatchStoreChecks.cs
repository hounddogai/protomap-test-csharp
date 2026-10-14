namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolBatch;

using Acme.Workforce.Compensation.BonusPool.PoolBatch;

/// <summary>Checks of pool batch code, run by the project's runner.</summary>
public static class TieredBonusPoolBatchStoreChecks
{
    public static void MemoryTieredBonusPoolBatchStoreFindsWhatItSaved()
    {
        var store = TieredBonusPoolBatchStore.NewMemoryTieredBonusPoolBatchStore();
        var item = TieredBonusPoolBatch.NewTieredBonusPoolBatch("code-832", "name-910");
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
        MemoryTieredBonusPoolBatchStoreFindsWhatItSaved();
    }
}
