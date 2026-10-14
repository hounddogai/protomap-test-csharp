namespace Acme.Workforce.Contractors.Tests.Rate.Batch;

using Acme.Workforce.Contractors.Rate.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class RateQuotaStoreChecks
{
    public static void MemoryRateQuotaStoreFindsWhatItSaved()
    {
        var store = RateQuotaStore.NewMemoryRateQuotaStore();
        var item = RateQuota.NewRateQuota("code-349", "name-915");
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
        MemoryRateQuotaStoreFindsWhatItSaved();
    }
}
