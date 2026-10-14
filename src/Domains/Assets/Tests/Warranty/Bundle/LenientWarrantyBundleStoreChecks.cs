namespace Acme.Workforce.Assets.Tests.Warranty.Bundle;

using Acme.Workforce.Assets.Warranty.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class LenientWarrantyBundleStoreChecks
{
    public static void MemoryLenientWarrantyBundleStoreFindsWhatItSaved()
    {
        var store = LenientWarrantyBundleStore.NewMemoryLenientWarrantyBundleStore();
        var item = LenientWarrantyBundle.NewLenientWarrantyBundle("code-551", "label-666");
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
        MemoryLenientWarrantyBundleStoreFindsWhatItSaved();
    }
}
