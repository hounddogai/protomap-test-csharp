namespace Acme.Workforce.Payrollops.Tests.PayRun.Metric;

using Acme.Workforce.Payrollops.PayRun.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class BatchedPayRunMetricStoreChecks
{
    public static void MemoryBatchedPayRunMetricStoreFindsWhatItSaved()
    {
        var store = BatchedPayRunMetricStore.NewMemoryBatchedPayRunMetricStore();
        var item = BatchedPayRunMetric.NewBatchedPayRunMetric("code-745", "title-405");
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
        MemoryBatchedPayRunMetricStoreFindsWhatItSaved();
    }
}
