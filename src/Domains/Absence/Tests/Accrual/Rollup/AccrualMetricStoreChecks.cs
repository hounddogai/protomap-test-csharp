namespace Acme.Workforce.Absence.Tests.Accrual.Rollup;

using Acme.Workforce.Absence.Accrual.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class AccrualMetricStoreChecks
{
    public static void MemoryAccrualMetricStoreFindsWhatItSaved()
    {
        var store = AccrualMetricStore.NewMemoryAccrualMetricStore();
        var item = AccrualMetric.NewAccrualMetric("code-885", "label-3");
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
        MemoryAccrualMetricStoreFindsWhatItSaved();
    }
}
