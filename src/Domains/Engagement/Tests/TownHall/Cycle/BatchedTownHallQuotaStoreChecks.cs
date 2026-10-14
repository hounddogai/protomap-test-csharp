namespace Acme.Workforce.Engagement.Tests.TownHall.Cycle;

using Acme.Workforce.Engagement.TownHall.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class BatchedTownHallQuotaStoreChecks
{
    public static void MemoryBatchedTownHallQuotaStoreFindsWhatItSaved()
    {
        var store = BatchedTownHallQuotaStore.NewMemoryBatchedTownHallQuotaStore();
        var item = BatchedTownHallQuota.NewBatchedTownHallQuota("code-787", "label-729");
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
        MemoryBatchedTownHallQuotaStoreFindsWhatItSaved();
    }
}
