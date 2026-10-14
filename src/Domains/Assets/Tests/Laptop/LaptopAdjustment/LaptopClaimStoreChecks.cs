namespace Acme.Workforce.Assets.Tests.Laptop.LaptopAdjustment;

using Acme.Workforce.Assets.Laptop.LaptopAdjustment;

/// <summary>Checks of laptop adjustment code, run by the project's runner.</summary>
public static class LaptopClaimStoreChecks
{
    public static void MemoryLaptopClaimStoreFindsWhatItSaved()
    {
        var store = LaptopClaimStore.NewMemoryLaptopClaimStore();
        var item = LaptopClaim.NewLaptopClaim("code-98", "name-931");
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
        MemoryLaptopClaimStoreFindsWhatItSaved();
    }
}
