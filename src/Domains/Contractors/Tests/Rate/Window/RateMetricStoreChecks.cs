namespace Acme.Workforce.Contractors.Tests.Rate.Window;

using Acme.Workforce.Contractors.Rate.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class RateMetricStoreChecks
{
    public static void MemoryRateMetricStoreFindsWhatItSaved()
    {
        var store = RateMetricStore.NewMemoryRateMetricStore();
        var item = RateMetric.NewRateMetric("code-341", "name-658");
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
        MemoryRateMetricStoreFindsWhatItSaved();
    }
}
