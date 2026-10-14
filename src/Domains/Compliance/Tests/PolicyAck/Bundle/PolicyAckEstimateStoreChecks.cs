namespace Acme.Workforce.Compliance.Tests.PolicyAck.Bundle;

using Acme.Workforce.Compliance.PolicyAck.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PolicyAckEstimateStoreChecks
{
    public static void MemoryPolicyAckEstimateStoreFindsWhatItSaved()
    {
        var store = PolicyAckEstimateStore.NewMemoryPolicyAckEstimateStore();
        var item = PolicyAckEstimate.NewPolicyAckEstimate("code-653", "name-366");
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
        MemoryPolicyAckEstimateStoreFindsWhatItSaved();
    }
}
