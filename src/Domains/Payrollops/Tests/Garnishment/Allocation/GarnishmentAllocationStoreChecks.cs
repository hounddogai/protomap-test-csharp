namespace Acme.Workforce.Payrollops.Tests.Garnishment.Allocation;

using Acme.Workforce.Payrollops.Garnishment.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class GarnishmentAllocationStoreChecks
{
    public static void MemoryGarnishmentAllocationStoreFindsWhatItSaved()
    {
        var store = GarnishmentAllocationStore.NewMemoryGarnishmentAllocationStore();
        var item = GarnishmentAllocation.NewGarnishmentAllocation("code-196", "title-532");
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
        MemoryGarnishmentAllocationStoreFindsWhatItSaved();
    }
}
