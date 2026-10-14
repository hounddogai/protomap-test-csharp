namespace Acme.Workforce.Payrollops.Tests.RetroPay.Claim;

using Acme.Workforce.Payrollops.RetroPay.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AdaptiveRetroPayMetricStoreChecks
{
    public static void MemoryAdaptiveRetroPayMetricStoreFindsWhatItSaved()
    {
        var store = AdaptiveRetroPayMetricStore.NewMemoryAdaptiveRetroPayMetricStore();
        var item = AdaptiveRetroPayMetric.NewAdaptiveRetroPayMetric("code-219", "title-716");
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
        MemoryAdaptiveRetroPayMetricStoreFindsWhatItSaved();
    }
}
