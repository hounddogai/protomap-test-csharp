namespace Acme.Workforce.Compliance.Tests.PolicyAck.Notice;

using Acme.Workforce.Compliance.PolicyAck.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class PolicyAckRollupStoreChecks
{
    public static void MemoryPolicyAckRollupStoreFindsWhatItSaved()
    {
        var store = PolicyAckRollupStore.NewMemoryPolicyAckRollupStore();
        var item = PolicyAckRollup.NewPolicyAckRollup("code-201", "name-370");
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
        MemoryPolicyAckRollupStoreFindsWhatItSaved();
    }
}
