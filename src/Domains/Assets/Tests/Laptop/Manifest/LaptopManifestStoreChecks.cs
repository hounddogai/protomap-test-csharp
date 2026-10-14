namespace Acme.Workforce.Assets.Tests.Laptop.Manifest;

using Acme.Workforce.Assets.Laptop.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class LaptopManifestStoreChecks
{
    public static void MemoryLaptopManifestStoreFindsWhatItSaved()
    {
        var store = LaptopManifestStore.NewMemoryLaptopManifestStore();
        var item = LaptopManifest.NewLaptopManifest("code-76", "title-288");
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
        MemoryLaptopManifestStoreFindsWhatItSaved();
    }
}
