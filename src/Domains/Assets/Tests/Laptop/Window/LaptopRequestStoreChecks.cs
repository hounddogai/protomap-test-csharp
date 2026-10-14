namespace Acme.Workforce.Assets.Tests.Laptop.Window;

using Acme.Workforce.Assets.Laptop.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class LaptopRequestStoreChecks
{
    public static void MemoryLaptopRequestStoreFindsWhatItSaved()
    {
        var store = LaptopRequestStore.NewMemoryLaptopRequestStore();
        var item = LaptopRequest.NewLaptopRequest("code-437", "label-488");
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
        MemoryLaptopRequestStoreFindsWhatItSaved();
    }
}
