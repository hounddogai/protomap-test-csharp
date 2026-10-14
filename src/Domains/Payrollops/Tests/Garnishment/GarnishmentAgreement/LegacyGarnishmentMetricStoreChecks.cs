namespace Acme.Workforce.Payrollops.Tests.Garnishment.GarnishmentAgreement;

using Acme.Workforce.Payrollops.Garnishment.GarnishmentAgreement;

/// <summary>Checks of garnishment agreement code, run by the project's runner.</summary>
public static class LegacyGarnishmentMetricStoreChecks
{
    public static void MemoryLegacyGarnishmentMetricStoreFindsWhatItSaved()
    {
        var store = LegacyGarnishmentMetricStore.NewMemoryLegacyGarnishmentMetricStore();
        var item = LegacyGarnishmentMetric.NewLegacyGarnishmentMetric("code-790", "label-64");
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
        MemoryLegacyGarnishmentMetricStoreFindsWhatItSaved();
    }
}
