namespace Acme.Workforce.Payrollops.Tests.Garnishment.Mapping;

using Acme.Workforce.Payrollops.Garnishment.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class AdaptiveGarnishmentAllocationStoreChecks
{
    public static void MemoryAdaptiveGarnishmentAllocationStoreFindsWhatItSaved()
    {
        var store = AdaptiveGarnishmentAllocationStore.NewMemoryAdaptiveGarnishmentAllocationStore();
        var item = AdaptiveGarnishmentAllocation.NewAdaptiveGarnishmentAllocation("code-445", "title-727");
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
        MemoryAdaptiveGarnishmentAllocationStoreFindsWhatItSaved();
    }
}
