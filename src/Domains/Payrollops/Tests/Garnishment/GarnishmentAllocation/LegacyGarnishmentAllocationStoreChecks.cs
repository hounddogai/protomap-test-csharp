namespace Acme.Workforce.Payrollops.Tests.Garnishment.GarnishmentAllocation;

using Acme.Workforce.Payrollops.Garnishment.GarnishmentAllocation;

/// <summary>Checks of garnishment allocation code, run by the project's runner.</summary>
public static class LegacyGarnishmentAllocationStoreChecks
{
    public static void MemoryLegacyGarnishmentAllocationStoreFindsWhatItSaved()
    {
        var store = LegacyGarnishmentAllocationStore.NewMemoryLegacyGarnishmentAllocationStore();
        var item = LegacyGarnishmentAllocation.NewLegacyGarnishmentAllocation("code-324", "label-575");
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
        MemoryLegacyGarnishmentAllocationStoreFindsWhatItSaved();
    }
}
