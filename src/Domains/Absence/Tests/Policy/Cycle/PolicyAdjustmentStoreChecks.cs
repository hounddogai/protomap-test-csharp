namespace Acme.Workforce.Absence.Tests.Policy.Cycle;

using Acme.Workforce.Absence.Policy.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class PolicyAdjustmentStoreChecks
{
    public static void MemoryPolicyAdjustmentStoreFindsWhatItSaved()
    {
        var store = PolicyAdjustmentStore.NewMemoryPolicyAdjustmentStore();
        var item = PolicyAdjustment.NewPolicyAdjustment("code-346", "title-519");
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
        MemoryPolicyAdjustmentStoreFindsWhatItSaved();
    }
}
