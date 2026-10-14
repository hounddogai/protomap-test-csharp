namespace Acme.Workforce.Orgdesign.Tests.Headcount.Allocation;

using Acme.Workforce.Orgdesign.Headcount.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class HeadcountAllocationStoreChecks
{
    public static void MemoryHeadcountAllocationStoreFindsWhatItSaved()
    {
        var store = HeadcountAllocationStore.NewMemoryHeadcountAllocationStore();
        var item = HeadcountAllocation.NewHeadcountAllocation("code-921", "label-17");
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
        MemoryHeadcountAllocationStoreFindsWhatItSaved();
    }
}
