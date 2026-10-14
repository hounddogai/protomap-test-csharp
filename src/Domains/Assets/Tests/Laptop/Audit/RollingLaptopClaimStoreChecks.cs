namespace Acme.Workforce.Assets.Tests.Laptop.Audit;

using Acme.Workforce.Assets.Laptop.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class RollingLaptopClaimStoreChecks
{
    public static void MemoryRollingLaptopClaimStoreFindsWhatItSaved()
    {
        var store = RollingLaptopClaimStore.NewMemoryRollingLaptopClaimStore();
        var item = RollingLaptopClaim.NewRollingLaptopClaim("code-455", "name-772");
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
        MemoryRollingLaptopClaimStoreFindsWhatItSaved();
    }
}
