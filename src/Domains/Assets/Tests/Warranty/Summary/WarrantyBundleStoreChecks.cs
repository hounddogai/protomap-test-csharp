namespace Acme.Workforce.Assets.Tests.Warranty.Summary;

using Acme.Workforce.Assets.Warranty.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class WarrantyBundleStoreChecks
{
    public static void MemoryWarrantyBundleStoreFindsWhatItSaved()
    {
        var store = WarrantyBundleStore.NewMemoryWarrantyBundleStore();
        var item = WarrantyBundle.NewWarrantyBundle("code-798", "title-380");
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
        MemoryWarrantyBundleStoreFindsWhatItSaved();
    }
}
