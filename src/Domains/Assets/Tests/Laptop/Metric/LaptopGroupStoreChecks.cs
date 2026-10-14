namespace Acme.Workforce.Assets.Tests.Laptop.Metric;

using Acme.Workforce.Assets.Laptop.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class LaptopGroupStoreChecks
{
    public static void MemoryLaptopGroupStoreFindsWhatItSaved()
    {
        var store = LaptopGroupStore.NewMemoryLaptopGroupStore();
        var item = LaptopGroup.NewLaptopGroup("code-667", "name-933");
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
        MemoryLaptopGroupStoreFindsWhatItSaved();
    }
}
