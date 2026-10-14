namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckCycle;

using Acme.Workforce.Compliance.PolicyAck.AckCycle;

/// <summary>Checks of ack cycle code, run by the project's runner.</summary>
public static class CappedPolicyAckRollupStoreChecks
{
    public static void MemoryCappedPolicyAckRollupStoreFindsWhatItSaved()
    {
        var store = CappedPolicyAckRollupStore.NewMemoryCappedPolicyAckRollupStore();
        var item = CappedPolicyAckRollup.NewCappedPolicyAckRollup("code-816", "title-300");
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
        MemoryCappedPolicyAckRollupStoreFindsWhatItSaved();
    }
}
