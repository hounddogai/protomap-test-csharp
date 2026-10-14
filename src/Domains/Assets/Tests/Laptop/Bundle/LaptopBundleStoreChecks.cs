namespace Acme.Workforce.Assets.Tests.Laptop.Bundle;

using Acme.Workforce.Assets.Laptop.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class LaptopBundleStoreChecks
{
    public static void MemoryLaptopBundleStoreFindsWhatItSaved()
    {
        var store = LaptopBundleStore.NewMemoryLaptopBundleStore();
        var item = LaptopBundle.NewLaptopBundle("code-510", "title-454");
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
        MemoryLaptopBundleStoreFindsWhatItSaved();
    }
}
