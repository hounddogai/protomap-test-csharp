namespace Acme.Workforce.Assets.Tests.Laptop.Milestone;

using Acme.Workforce.Assets.Laptop.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LaptopTierStoreChecks
{
    public static void MemoryLaptopTierStoreFindsWhatItSaved()
    {
        var store = LaptopTierStore.NewMemoryLaptopTierStore();
        var item = LaptopTier.NewLaptopTier("code-469", "title-319");
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
        MemoryLaptopTierStoreFindsWhatItSaved();
    }
}
