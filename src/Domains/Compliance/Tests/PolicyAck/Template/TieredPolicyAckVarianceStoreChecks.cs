namespace Acme.Workforce.Compliance.Tests.PolicyAck.Template;

using Acme.Workforce.Compliance.PolicyAck.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class TieredPolicyAckVarianceStoreChecks
{
    public static void MemoryTieredPolicyAckVarianceStoreFindsWhatItSaved()
    {
        var store = TieredPolicyAckVarianceStore.NewMemoryTieredPolicyAckVarianceStore();
        var item = TieredPolicyAckVariance.NewTieredPolicyAckVariance("code-911", "label-24");
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
        MemoryTieredPolicyAckVarianceStoreFindsWhatItSaved();
    }
}
