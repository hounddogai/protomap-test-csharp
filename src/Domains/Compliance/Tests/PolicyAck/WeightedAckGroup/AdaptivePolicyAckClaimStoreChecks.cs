namespace Acme.Workforce.Compliance.Tests.PolicyAck.WeightedAckGroup;

using Acme.Workforce.Compliance.PolicyAck.WeightedAckGroup;

/// <summary>Checks of weighted ack group code, run by the project's runner.</summary>
public static class AdaptivePolicyAckClaimStoreChecks
{
    public static void MemoryAdaptivePolicyAckClaimStoreFindsWhatItSaved()
    {
        var store = AdaptivePolicyAckClaimStore.NewMemoryAdaptivePolicyAckClaimStore();
        var item = AdaptivePolicyAckClaim.NewAdaptivePolicyAckClaim("code-581", "label-804");
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
        MemoryAdaptivePolicyAckClaimStoreFindsWhatItSaved();
    }
}
