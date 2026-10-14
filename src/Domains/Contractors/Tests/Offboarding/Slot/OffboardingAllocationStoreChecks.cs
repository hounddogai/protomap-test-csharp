namespace Acme.Workforce.Contractors.Tests.Offboarding.Slot;

using Acme.Workforce.Contractors.Offboarding.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class OffboardingAllocationStoreChecks
{
    public static void MemoryOffboardingAllocationStoreFindsWhatItSaved()
    {
        var store = OffboardingAllocationStore.NewMemoryOffboardingAllocationStore();
        var item = OffboardingAllocation.NewOffboardingAllocation("code-143", "name-13");
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
        MemoryOffboardingAllocationStoreFindsWhatItSaved();
    }
}
