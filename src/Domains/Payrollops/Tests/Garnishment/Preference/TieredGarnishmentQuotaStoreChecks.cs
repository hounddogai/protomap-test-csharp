namespace Acme.Workforce.Payrollops.Tests.Garnishment.Preference;

using Acme.Workforce.Payrollops.Garnishment.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class TieredGarnishmentQuotaStoreChecks
{
    public static void MemoryTieredGarnishmentQuotaStoreFindsWhatItSaved()
    {
        var store = TieredGarnishmentQuotaStore.NewMemoryTieredGarnishmentQuotaStore();
        var item = TieredGarnishmentQuota.NewTieredGarnishmentQuota("code-691", "title-669");
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
        MemoryTieredGarnishmentQuotaStoreFindsWhatItSaved();
    }
}
