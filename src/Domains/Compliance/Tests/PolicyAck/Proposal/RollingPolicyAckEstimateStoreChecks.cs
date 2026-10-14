namespace Acme.Workforce.Compliance.Tests.PolicyAck.Proposal;

using Acme.Workforce.Compliance.PolicyAck.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class RollingPolicyAckEstimateStoreChecks
{
    public static void MemoryRollingPolicyAckEstimateStoreFindsWhatItSaved()
    {
        var store = RollingPolicyAckEstimateStore.NewMemoryRollingPolicyAckEstimateStore();
        var item = RollingPolicyAckEstimate.NewRollingPolicyAckEstimate("code-286", "title-292");
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
        MemoryRollingPolicyAckEstimateStoreFindsWhatItSaved();
    }
}
