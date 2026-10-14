namespace Acme.Workforce.Assets.Tests.Warranty.Budget;

using Acme.Workforce.Assets.Warranty.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class AdaptiveWarrantyBundleStoreChecks
{
    public static void MemoryAdaptiveWarrantyBundleStoreFindsWhatItSaved()
    {
        var store = AdaptiveWarrantyBundleStore.NewMemoryAdaptiveWarrantyBundleStore();
        var item = AdaptiveWarrantyBundle.NewAdaptiveWarrantyBundle("code-54", "name-825");
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
        MemoryAdaptiveWarrantyBundleStoreFindsWhatItSaved();
    }
}
