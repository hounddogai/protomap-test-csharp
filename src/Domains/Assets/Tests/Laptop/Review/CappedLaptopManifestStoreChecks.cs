namespace Acme.Workforce.Assets.Tests.Laptop.Review;

using Acme.Workforce.Assets.Laptop.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class CappedLaptopManifestStoreChecks
{
    public static void MemoryCappedLaptopManifestStoreFindsWhatItSaved()
    {
        var store = CappedLaptopManifestStore.NewMemoryCappedLaptopManifestStore();
        var item = CappedLaptopManifest.NewCappedLaptopManifest("code-621", "name-317");
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
        MemoryCappedLaptopManifestStoreFindsWhatItSaved();
    }
}
