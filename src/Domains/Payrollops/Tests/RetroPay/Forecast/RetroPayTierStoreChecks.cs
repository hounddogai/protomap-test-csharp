namespace Acme.Workforce.Payrollops.Tests.RetroPay.Forecast;

using Acme.Workforce.Payrollops.RetroPay.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class RetroPayTierStoreChecks
{
    public static void MemoryRetroPayTierStoreFindsWhatItSaved()
    {
        var store = RetroPayTierStore.NewMemoryRetroPayTierStore();
        var item = RetroPayTier.NewRetroPayTier("code-252", "label-61");
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
        MemoryRetroPayTierStoreFindsWhatItSaved();
    }
}
