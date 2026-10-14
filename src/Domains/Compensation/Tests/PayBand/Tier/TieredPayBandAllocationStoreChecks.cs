namespace Acme.Workforce.Compensation.Tests.PayBand.Tier;

using Acme.Workforce.Compensation.PayBand.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class TieredPayBandAllocationStoreChecks
{
    public static void MemoryTieredPayBandAllocationStoreFindsWhatItSaved()
    {
        var store = TieredPayBandAllocationStore.NewMemoryTieredPayBandAllocationStore();
        var item = TieredPayBandAllocation.NewTieredPayBandAllocation("code-198", "name-59");
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
        MemoryTieredPayBandAllocationStoreFindsWhatItSaved();
    }
}
