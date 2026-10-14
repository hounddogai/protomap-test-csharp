namespace Acme.Workforce.Contractors.Tests.Rate.RateCycle;

using Acme.Workforce.Contractors.Rate.RateCycle;

/// <summary>Checks of rate cycle code, run by the project's runner.</summary>
public static class WeightedRateCycleStoreChecks
{
    public static void MemoryWeightedRateCycleStoreFindsWhatItSaved()
    {
        var store = WeightedRateCycleStore.NewMemoryWeightedRateCycleStore();
        var item = WeightedRateCycle.NewWeightedRateCycle("code-628", "name-265");
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
        MemoryWeightedRateCycleStoreFindsWhatItSaved();
    }
}
