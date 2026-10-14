namespace Acme.Workforce.Absence.Tests.Policy.Threshold;

using Acme.Workforce.Absence.Policy.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PolicyAllocationStoreChecks
{
    public static void MemoryPolicyAllocationStoreFindsWhatItSaved()
    {
        var store = PolicyAllocationStore.NewMemoryPolicyAllocationStore();
        var item = PolicyAllocation.NewPolicyAllocation("code-111", "label-447");
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
        MemoryPolicyAllocationStoreFindsWhatItSaved();
    }
}
