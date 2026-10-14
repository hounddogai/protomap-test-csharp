namespace Acme.Workforce.Compensation.Tests.EquityGrant.Metric;

using Acme.Workforce.Compensation.EquityGrant.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class EquityGrantMetricStoreChecks
{
    public static void MemoryEquityGrantMetricStoreFindsWhatItSaved()
    {
        var store = EquityGrantMetricStore.NewMemoryEquityGrantMetricStore();
        var item = EquityGrantMetric.NewEquityGrantMetric("code-770", "label-967");
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
        MemoryEquityGrantMetricStoreFindsWhatItSaved();
    }
}
