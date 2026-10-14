namespace Acme.Workforce.Payrollops.Tests.PayRun.Metric;

using Acme.Workforce.Payrollops.PayRun.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class PayRunMetricStoreChecks
{
    public static void MemoryPayRunMetricStoreFindsWhatItSaved()
    {
        var store = PayRunMetricStore.NewMemoryPayRunMetricStore();
        var item = PayRunMetric.NewPayRunMetric("code-130", "title-81");
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
        MemoryPayRunMetricStoreFindsWhatItSaved();
    }
}
