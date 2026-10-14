namespace Acme.Workforce.Compliance.Tests.PolicyAck.Profile;

using Acme.Workforce.Compliance.PolicyAck.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PendingPolicyAckGroupStoreChecks
{
    public static void MemoryPendingPolicyAckGroupStoreFindsWhatItSaved()
    {
        var store = PendingPolicyAckGroupStore.NewMemoryPendingPolicyAckGroupStore();
        var item = PendingPolicyAckGroup.NewPendingPolicyAckGroup("code-154", "title-79");
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
        MemoryPendingPolicyAckGroupStoreFindsWhatItSaved();
    }
}
