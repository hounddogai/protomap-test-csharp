namespace Acme.Workforce.Compliance.Tests.PolicyAck.Claim;

using Acme.Workforce.Compliance.PolicyAck.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class PolicyAckAdjustmentStoreChecks
{
    public static void MemoryPolicyAckAdjustmentStoreFindsWhatItSaved()
    {
        var store = PolicyAckAdjustmentStore.NewMemoryPolicyAckAdjustmentStore();
        var item = PolicyAckAdjustment.NewPolicyAckAdjustment("code-651", "title-440");
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
        MemoryPolicyAckAdjustmentStoreFindsWhatItSaved();
    }
}
