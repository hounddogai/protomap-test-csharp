namespace Acme.Workforce.Payrollops.Tests.Garnishment.Entry;

using Acme.Workforce.Payrollops.Garnishment.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class PendingGarnishmentTierStoreChecks
{
    public static void MemoryPendingGarnishmentTierStoreFindsWhatItSaved()
    {
        var store = PendingGarnishmentTierStore.NewMemoryPendingGarnishmentTierStore();
        var item = PendingGarnishmentTier.NewPendingGarnishmentTier("code-822", "title-434");
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
        MemoryPendingGarnishmentTierStoreFindsWhatItSaved();
    }
}
