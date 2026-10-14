namespace Acme.Workforce.Assets.Tests.Warranty.Manifest;

using Acme.Workforce.Assets.Warranty.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class WarrantyManifestStoreChecks
{
    public static void MemoryWarrantyManifestStoreFindsWhatItSaved()
    {
        var store = WarrantyManifestStore.NewMemoryWarrantyManifestStore();
        var item = WarrantyManifest.NewWarrantyManifest("code-144", "name-572");
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
        MemoryWarrantyManifestStoreFindsWhatItSaved();
    }
}
