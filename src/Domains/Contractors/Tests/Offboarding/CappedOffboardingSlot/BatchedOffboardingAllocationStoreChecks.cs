namespace Acme.Workforce.Contractors.Tests.Offboarding.CappedOffboardingSlot;

using Acme.Workforce.Contractors.Offboarding.CappedOffboardingSlot;

/// <summary>Checks of capped offboarding slot code, run by the project's runner.</summary>
public static class BatchedOffboardingAllocationStoreChecks
{
    public static void MemoryBatchedOffboardingAllocationStoreFindsWhatItSaved()
    {
        var store = BatchedOffboardingAllocationStore.NewMemoryBatchedOffboardingAllocationStore();
        var item = BatchedOffboardingAllocation.NewBatchedOffboardingAllocation("code-632", "label-224");
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
        MemoryBatchedOffboardingAllocationStoreFindsWhatItSaved();
    }
}
