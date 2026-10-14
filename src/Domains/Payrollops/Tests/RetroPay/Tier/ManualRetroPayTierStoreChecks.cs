namespace Acme.Workforce.Payrollops.Tests.RetroPay.Tier;

using Acme.Workforce.Payrollops.RetroPay.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class ManualRetroPayTierStoreChecks
{
    public static void MemoryManualRetroPayTierStoreFindsWhatItSaved()
    {
        var store = ManualRetroPayTierStore.NewMemoryManualRetroPayTierStore();
        var item = ManualRetroPayTier.NewManualRetroPayTier("code-434", "name-959");
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
        MemoryManualRetroPayTierStoreFindsWhatItSaved();
    }
}
