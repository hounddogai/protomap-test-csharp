namespace Acme.Workforce.Assets.Tests.Laptop.Mapping;

using Acme.Workforce.Assets.Laptop.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class DefaultLaptopWindowStoreChecks
{
    public static void MemoryDefaultLaptopWindowStoreFindsWhatItSaved()
    {
        var store = DefaultLaptopWindowStore.NewMemoryDefaultLaptopWindowStore();
        var item = DefaultLaptopWindow.NewDefaultLaptopWindow("code-130", "label-387");
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
        MemoryDefaultLaptopWindowStoreFindsWhatItSaved();
    }
}
