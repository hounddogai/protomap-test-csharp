namespace Acme.Workforce.Assets.Tests.Laptop.Manifest;

using Acme.Workforce.Assets.Laptop.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class TieredLaptopTierStoreChecks
{
    public static void MemoryTieredLaptopTierStoreFindsWhatItSaved()
    {
        var store = TieredLaptopTierStore.NewMemoryTieredLaptopTierStore();
        var item = TieredLaptopTier.NewTieredLaptopTier("code-562", "label-317");
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
        MemoryTieredLaptopTierStoreFindsWhatItSaved();
    }
}
