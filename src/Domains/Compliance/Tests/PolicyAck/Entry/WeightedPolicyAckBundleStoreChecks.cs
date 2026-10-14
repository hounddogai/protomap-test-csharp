namespace Acme.Workforce.Compliance.Tests.PolicyAck.Entry;

using Acme.Workforce.Compliance.PolicyAck.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class WeightedPolicyAckBundleStoreChecks
{
    public static void MemoryWeightedPolicyAckBundleStoreFindsWhatItSaved()
    {
        var store = WeightedPolicyAckBundleStore.NewMemoryWeightedPolicyAckBundleStore();
        var item = WeightedPolicyAckBundle.NewWeightedPolicyAckBundle("code-305", "label-926");
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
        MemoryWeightedPolicyAckBundleStoreFindsWhatItSaved();
    }
}
