namespace Acme.Workforce.Compensation.Tests.EquityGrant.Batch;

using Acme.Workforce.Compensation.EquityGrant.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class StrictEquityGrantMetricStoreChecks
{
    public static void MemoryStrictEquityGrantMetricStoreFindsWhatItSaved()
    {
        var store = StrictEquityGrantMetricStore.NewMemoryStrictEquityGrantMetricStore();
        var item = StrictEquityGrantMetric.NewStrictEquityGrantMetric("code-840", "title-14");
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
        MemoryStrictEquityGrantMetricStoreFindsWhatItSaved();
    }
}
