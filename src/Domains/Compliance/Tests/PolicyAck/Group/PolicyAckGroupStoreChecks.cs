namespace Acme.Workforce.Compliance.Tests.PolicyAck.Group;

using Acme.Workforce.Compliance.PolicyAck.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PolicyAckGroupStoreChecks
{
    public static void MemoryPolicyAckGroupStoreFindsWhatItSaved()
    {
        var store = PolicyAckGroupStore.NewMemoryPolicyAckGroupStore();
        var item = PolicyAckGroup.NewPolicyAckGroup("code-712", "title-722");
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
        MemoryPolicyAckGroupStoreFindsWhatItSaved();
    }
}
