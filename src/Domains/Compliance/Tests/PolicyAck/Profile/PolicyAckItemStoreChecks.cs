namespace Acme.Workforce.Compliance.Tests.PolicyAck.Profile;

using Acme.Workforce.Compliance.PolicyAck.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PolicyAckItemStoreChecks
{
    public static void MemoryPolicyAckItemStoreFindsWhatItSaved()
    {
        var store = PolicyAckItemStore.NewMemoryPolicyAckItemStore();
        var item = PolicyAckItem.NewPolicyAckItem("code-191", "label-695");
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
        MemoryPolicyAckItemStoreFindsWhatItSaved();
    }
}
