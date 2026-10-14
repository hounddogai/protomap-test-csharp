namespace Acme.Workforce.Compliance.Tests.Exception.Allocation;

using Acme.Workforce.Compliance.Exception.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class PendingExceptionSlotStoreChecks
{
    public static void MemoryPendingExceptionSlotStoreFindsWhatItSaved()
    {
        var store = PendingExceptionSlotStore.NewMemoryPendingExceptionSlotStore();
        var item = PendingExceptionSlot.NewPendingExceptionSlot("code-412", "title-245");
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
        MemoryPendingExceptionSlotStoreFindsWhatItSaved();
    }
}
