namespace Acme.Workforce.Workplace.Tests.Desk.Variance;

using Acme.Workforce.Workplace.Desk.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class LenientDeskAllocationStoreChecks
{
    public static void MemoryLenientDeskAllocationStoreFindsWhatItSaved()
    {
        var store = LenientDeskAllocationStore.NewMemoryLenientDeskAllocationStore();
        var item = LenientDeskAllocation.NewLenientDeskAllocation("code-237", "title-827");
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
        MemoryLenientDeskAllocationStoreFindsWhatItSaved();
    }
}
