namespace Acme.Workforce.Compensation.Tests.MeritCycle.Allocation;

using Acme.Workforce.Compensation.MeritCycle.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class MeritCycleAllocationStoreChecks
{
    public static void MemoryMeritCycleAllocationStoreFindsWhatItSaved()
    {
        var store = MeritCycleAllocationStore.NewMemoryMeritCycleAllocationStore();
        var item = MeritCycleAllocation.NewMeritCycleAllocation("code-941", "title-869");
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
        MemoryMeritCycleAllocationStoreFindsWhatItSaved();
    }
}
