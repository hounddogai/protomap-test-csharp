namespace Acme.Workforce.Payrollops.Tests.Garnishment.Plan;

using Acme.Workforce.Payrollops.Garnishment.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class GarnishmentMetricStoreChecks
{
    public static void MemoryGarnishmentMetricStoreFindsWhatItSaved()
    {
        var store = GarnishmentMetricStore.NewMemoryGarnishmentMetricStore();
        var item = GarnishmentMetric.NewGarnishmentMetric("code-796", "name-986");
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
        MemoryGarnishmentMetricStoreFindsWhatItSaved();
    }
}
