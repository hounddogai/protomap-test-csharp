namespace Acme.Workforce.Workplace.Tests.Visitor.Slot;

using Acme.Workforce.Workplace.Visitor.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class VisitorAllocationStoreChecks
{
    public static void MemoryVisitorAllocationStoreFindsWhatItSaved()
    {
        var store = VisitorAllocationStore.NewMemoryVisitorAllocationStore();
        var item = VisitorAllocation.NewVisitorAllocation("code-514", "label-597");
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
        MemoryVisitorAllocationStoreFindsWhatItSaved();
    }
}
