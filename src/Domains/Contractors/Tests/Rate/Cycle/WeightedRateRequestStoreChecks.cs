namespace Acme.Workforce.Contractors.Tests.Rate.Cycle;

using Acme.Workforce.Contractors.Rate.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class WeightedRateRequestStoreChecks
{
    public static void MemoryWeightedRateRequestStoreFindsWhatItSaved()
    {
        var store = WeightedRateRequestStore.NewMemoryWeightedRateRequestStore();
        var item = WeightedRateRequest.NewWeightedRateRequest("code-71", "title-999");
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
        MemoryWeightedRateRequestStoreFindsWhatItSaved();
    }
}
