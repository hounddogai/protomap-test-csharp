namespace Acme.Workforce.Assets.Tests.Laptop.Threshold;

using Acme.Workforce.Assets.Laptop.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LaptopDigestStoreChecks
{
    public static void MemoryLaptopDigestStoreFindsWhatItSaved()
    {
        var store = LaptopDigestStore.NewMemoryLaptopDigestStore();
        var item = LaptopDigest.NewLaptopDigest("code-766", "name-618");
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
        MemoryLaptopDigestStoreFindsWhatItSaved();
    }
}
