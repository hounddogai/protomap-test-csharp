namespace Acme.Workforce.Orgdesign.Tests.Reorg.Schedule;

using Acme.Workforce.Orgdesign.Reorg.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class ReorgAllocationStoreChecks
{
    public static void MemoryReorgAllocationStoreFindsWhatItSaved()
    {
        var store = ReorgAllocationStore.NewMemoryReorgAllocationStore();
        var item = ReorgAllocation.NewReorgAllocation("code-961", "name-701");
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
        MemoryReorgAllocationStoreFindsWhatItSaved();
    }
}
