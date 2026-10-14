namespace Acme.Workforce.Compliance.Tests.PolicyAck.Entry;

using Acme.Workforce.Compliance.PolicyAck.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class PolicyAckTemplateStoreChecks
{
    public static void MemoryPolicyAckTemplateStoreFindsWhatItSaved()
    {
        var store = PolicyAckTemplateStore.NewMemoryPolicyAckTemplateStore();
        var item = PolicyAckTemplate.NewPolicyAckTemplate("code-718", "name-307");
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
        MemoryPolicyAckTemplateStoreFindsWhatItSaved();
    }
}
