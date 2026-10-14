namespace Acme.Workforce.Absence.Tests.Accrual.Template;

using Acme.Workforce.Absence.Accrual.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class BatchedAccrualMetricStoreChecks
{
    public static void MemoryBatchedAccrualMetricStoreFindsWhatItSaved()
    {
        var store = BatchedAccrualMetricStore.NewMemoryBatchedAccrualMetricStore();
        var item = BatchedAccrualMetric.NewBatchedAccrualMetric("code-250", "label-185");
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
        MemoryBatchedAccrualMetricStoreFindsWhatItSaved();
    }
}
