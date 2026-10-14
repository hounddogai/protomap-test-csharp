namespace Acme.Workforce.Payrollops.Tests.Garnishment.DefaultGarnishmentAllocation;

using Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

/// <summary>Checks of default garnishment allocation code, run by the project's runner.</summary>
public static class WeightedGarnishmentVarianceStoreChecks
{
    public static void MemoryWeightedGarnishmentVarianceStoreFindsWhatItSaved()
    {
        var store = WeightedGarnishmentVarianceStore.NewMemoryWeightedGarnishmentVarianceStore();
        var item = WeightedGarnishmentVariance.NewWeightedGarnishmentVariance("code-379", "title-522");
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
        MemoryWeightedGarnishmentVarianceStoreFindsWhatItSaved();
    }
}
