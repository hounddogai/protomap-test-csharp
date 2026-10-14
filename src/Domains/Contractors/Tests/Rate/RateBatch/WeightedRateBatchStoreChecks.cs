namespace Acme.Workforce.Contractors.Tests.Rate.RateBatch;

using Acme.Workforce.Contractors.Rate.RateBatch;

/// <summary>Checks of rate batch code, run by the project's runner.</summary>
public static class WeightedRateBatchStoreChecks
{
    public static void MemoryWeightedRateBatchStoreFindsWhatItSaved()
    {
        var store = WeightedRateBatchStore.NewMemoryWeightedRateBatchStore();
        var item = WeightedRateBatch.NewWeightedRateBatch("code-229", "title-480");
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
        MemoryWeightedRateBatchStoreFindsWhatItSaved();
    }
}
