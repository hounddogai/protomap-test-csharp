namespace Acme.Workforce.Assets.Tests.Laptop.Audit;

using Acme.Workforce.Assets.Laptop.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class LenientLaptopWindowStoreChecks
{
    public static void MemoryLenientLaptopWindowStoreFindsWhatItSaved()
    {
        var store = LenientLaptopWindowStore.NewMemoryLenientLaptopWindowStore();
        var item = LenientLaptopWindow.NewLenientLaptopWindow("code-360", "name-368");
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
        MemoryLenientLaptopWindowStoreFindsWhatItSaved();
    }
}
