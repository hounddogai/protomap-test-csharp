namespace Acme.Workforce.Compliance.Tests.PolicyAck.Cycle;

using Acme.Workforce.Compliance.PolicyAck.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class ManualPolicyAckGroupStoreChecks
{
    public static void MemoryManualPolicyAckGroupStoreFindsWhatItSaved()
    {
        var store = ManualPolicyAckGroupStore.NewMemoryManualPolicyAckGroupStore();
        var item = ManualPolicyAckGroup.NewManualPolicyAckGroup("code-674", "name-237");
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
        MemoryManualPolicyAckGroupStoreFindsWhatItSaved();
    }
}
