namespace Acme.Workforce.Absence.Tests.Policy.PolicyEstimate;

using Acme.Workforce.Absence.Policy.PolicyEstimate;

/// <summary>Checks of policy estimate code, run by the project's runner.</summary>
public static class PendingPolicyAdjustmentStoreChecks
{
    public static void MemoryPendingPolicyAdjustmentStoreFindsWhatItSaved()
    {
        var store = PendingPolicyAdjustmentStore.NewMemoryPendingPolicyAdjustmentStore();
        var item = PendingPolicyAdjustment.NewPendingPolicyAdjustment("code-482", "title-215");
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
        MemoryPendingPolicyAdjustmentStoreFindsWhatItSaved();
    }
}
