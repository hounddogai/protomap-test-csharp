namespace Acme.Workforce.Assets.Tests.LicenseSeat.Bundle;

using Acme.Workforce.Assets.LicenseSeat.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class LicenseSeatAllocationStoreChecks
{
    public static void MemoryLicenseSeatAllocationStoreFindsWhatItSaved()
    {
        var store = LicenseSeatAllocationStore.NewMemoryLicenseSeatAllocationStore();
        var item = LicenseSeatAllocation.NewLicenseSeatAllocation("code-550", "label-156");
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
        MemoryLicenseSeatAllocationStoreFindsWhatItSaved();
    }
}
