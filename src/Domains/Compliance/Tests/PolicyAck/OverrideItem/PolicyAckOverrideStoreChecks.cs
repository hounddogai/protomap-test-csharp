namespace Acme.Workforce.Compliance.Tests.PolicyAck.OverrideItem;

using Acme.Workforce.Compliance.PolicyAck.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class PolicyAckOverrideStoreChecks
{
    public static void MemoryPolicyAckOverrideStoreFindsWhatItSaved()
    {
        var store = PolicyAckOverrideStore.NewMemoryPolicyAckOverrideStore();
        var item = PolicyAckOverride.NewPolicyAckOverride("code-575", "label-308");
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
        MemoryPolicyAckOverrideStoreFindsWhatItSaved();
    }
}
