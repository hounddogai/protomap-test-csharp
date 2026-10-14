namespace Acme.Workforce.Workplace.Tests.Desk.Allocation;

using Acme.Workforce.Workplace.Desk.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class DeskAllocationStoreChecks
{
    public static void MemoryDeskAllocationStoreFindsWhatItSaved()
    {
        var store = DeskAllocationStore.NewMemoryDeskAllocationStore();
        var item = DeskAllocation.NewDeskAllocation("code-763", "title-881");
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
        MemoryDeskAllocationStoreFindsWhatItSaved();
    }
}
