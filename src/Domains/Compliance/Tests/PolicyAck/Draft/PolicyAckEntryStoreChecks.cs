namespace Acme.Workforce.Compliance.Tests.PolicyAck.Draft;

using Acme.Workforce.Compliance.PolicyAck.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class PolicyAckEntryStoreChecks
{
    public static void MemoryPolicyAckEntryStoreFindsWhatItSaved()
    {
        var store = PolicyAckEntryStore.NewMemoryPolicyAckEntryStore();
        var item = PolicyAckEntry.NewPolicyAckEntry("code-245", "label-807");
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
        MemoryPolicyAckEntryStoreFindsWhatItSaved();
    }
}
