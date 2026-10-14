namespace Acme.Workforce.Compensation.Tests.BonusPool.Entry;

using Acme.Workforce.Compensation.BonusPool.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class BatchedBonusPoolThresholdStoreChecks
{
    public static void MemoryBatchedBonusPoolThresholdStoreFindsWhatItSaved()
    {
        var store = BatchedBonusPoolThresholdStore.NewMemoryBatchedBonusPoolThresholdStore();
        var item = BatchedBonusPoolThreshold.NewBatchedBonusPoolThreshold("code-983", "title-737");
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
        MemoryBatchedBonusPoolThresholdStoreFindsWhatItSaved();
    }
}
