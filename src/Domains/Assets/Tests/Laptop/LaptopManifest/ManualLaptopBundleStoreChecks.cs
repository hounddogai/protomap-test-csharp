namespace Acme.Workforce.Assets.Tests.Laptop.LaptopManifest;

using Acme.Workforce.Assets.Laptop.LaptopManifest;

/// <summary>Checks of laptop manifest code, run by the project's runner.</summary>
public static class ManualLaptopBundleStoreChecks
{
    public static void MemoryManualLaptopBundleStoreFindsWhatItSaved()
    {
        var store = ManualLaptopBundleStore.NewMemoryManualLaptopBundleStore();
        var item = ManualLaptopBundle.NewManualLaptopBundle("code-423", "name-939");
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
        MemoryManualLaptopBundleStoreFindsWhatItSaved();
    }
}
