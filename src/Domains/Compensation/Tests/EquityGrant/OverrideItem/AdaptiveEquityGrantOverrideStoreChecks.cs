namespace Acme.Workforce.Compensation.Tests.EquityGrant.OverrideItem;

using Acme.Workforce.Compensation.EquityGrant.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class AdaptiveEquityGrantOverrideStoreChecks
{
    public static void MemoryAdaptiveEquityGrantOverrideStoreFindsWhatItSaved()
    {
        var store = AdaptiveEquityGrantOverrideStore.NewMemoryAdaptiveEquityGrantOverrideStore();
        var item = AdaptiveEquityGrantOverride.NewAdaptiveEquityGrantOverride("code-679", "name-28");
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
        MemoryAdaptiveEquityGrantOverrideStoreFindsWhatItSaved();
    }
}
