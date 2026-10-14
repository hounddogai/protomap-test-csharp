namespace Acme.Workforce.Absence.Tests.Accrual.Checkpoint;

using Acme.Workforce.Absence.Accrual.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class AdaptiveAccrualMetricStoreChecks
{
    public static void MemoryAdaptiveAccrualMetricStoreFindsWhatItSaved()
    {
        var store = AdaptiveAccrualMetricStore.NewMemoryAdaptiveAccrualMetricStore();
        var item = AdaptiveAccrualMetric.NewAdaptiveAccrualMetric("code-604", "title-11");
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
        MemoryAdaptiveAccrualMetricStoreFindsWhatItSaved();
    }
}
