namespace Acme.Workforce.Payrollops.Tests.Garnishment.Allocation;

using Acme.Workforce.Payrollops.Garnishment.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class BatchedGarnishmentMetricStoreChecks
{
    public static void MemoryBatchedGarnishmentMetricStoreFindsWhatItSaved()
    {
        var store = BatchedGarnishmentMetricStore.NewMemoryBatchedGarnishmentMetricStore();
        var item = BatchedGarnishmentMetric.NewBatchedGarnishmentMetric("code-791", "name-181");
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
        MemoryBatchedGarnishmentMetricStoreFindsWhatItSaved();
    }
}
