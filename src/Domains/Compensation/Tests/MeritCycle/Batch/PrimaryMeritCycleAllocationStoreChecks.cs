namespace Acme.Workforce.Compensation.Tests.MeritCycle.Batch;

using Acme.Workforce.Compensation.MeritCycle.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PrimaryMeritCycleAllocationStoreChecks
{
    public static void MemoryPrimaryMeritCycleAllocationStoreFindsWhatItSaved()
    {
        var store = PrimaryMeritCycleAllocationStore.NewMemoryPrimaryMeritCycleAllocationStore();
        var item = PrimaryMeritCycleAllocation.NewPrimaryMeritCycleAllocation("code-380", "title-682");
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
        MemoryPrimaryMeritCycleAllocationStoreFindsWhatItSaved();
    }
}
