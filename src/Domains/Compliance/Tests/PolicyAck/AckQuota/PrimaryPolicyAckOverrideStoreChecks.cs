namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckQuota;

using Acme.Workforce.Compliance.PolicyAck.AckQuota;

/// <summary>Checks of ack quota code, run by the project's runner.</summary>
public static class PrimaryPolicyAckOverrideStoreChecks
{
    public static void MemoryPrimaryPolicyAckOverrideStoreFindsWhatItSaved()
    {
        var store = PrimaryPolicyAckOverrideStore.NewMemoryPrimaryPolicyAckOverrideStore();
        var item = PrimaryPolicyAckOverride.NewPrimaryPolicyAckOverride("code-681", "title-831");
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
        MemoryPrimaryPolicyAckOverrideStoreFindsWhatItSaved();
    }
}
