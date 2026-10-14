namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckProfile;

using Acme.Workforce.Compliance.PolicyAck.AckProfile;

/// <summary>Checks of ack profile code, run by the project's runner.</summary>
public static class PolicyAckVarianceStoreChecks
{
    public static void MemoryPolicyAckVarianceStoreFindsWhatItSaved()
    {
        var store = PolicyAckVarianceStore.NewMemoryPolicyAckVarianceStore();
        var item = PolicyAckVariance.NewPolicyAckVariance("code-941", "title-2");
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
        MemoryPolicyAckVarianceStoreFindsWhatItSaved();
    }
}
