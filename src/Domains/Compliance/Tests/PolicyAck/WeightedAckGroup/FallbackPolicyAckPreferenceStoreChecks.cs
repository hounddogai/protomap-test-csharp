namespace Acme.Workforce.Compliance.Tests.PolicyAck.WeightedAckGroup;

using Acme.Workforce.Compliance.PolicyAck.WeightedAckGroup;

/// <summary>Checks of weighted ack group code, run by the project's runner.</summary>
public static class FallbackPolicyAckPreferenceStoreChecks
{
    public static void MemoryFallbackPolicyAckPreferenceStoreFindsWhatItSaved()
    {
        var store = FallbackPolicyAckPreferenceStore.NewMemoryFallbackPolicyAckPreferenceStore();
        var item = FallbackPolicyAckPreference.NewFallbackPolicyAckPreference("code-192", "title-859");
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
        MemoryFallbackPolicyAckPreferenceStoreFindsWhatItSaved();
    }
}
