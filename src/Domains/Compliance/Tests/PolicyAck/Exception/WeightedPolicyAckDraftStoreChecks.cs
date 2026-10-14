namespace Acme.Workforce.Compliance.Tests.PolicyAck.Exception;

using Acme.Workforce.Compliance.PolicyAck.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class WeightedPolicyAckDraftStoreChecks
{
    public static void MemoryWeightedPolicyAckDraftStoreFindsWhatItSaved()
    {
        var store = WeightedPolicyAckDraftStore.NewMemoryWeightedPolicyAckDraftStore();
        var item = WeightedPolicyAckDraft.NewWeightedPolicyAckDraft("code-73", "title-949");
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
        MemoryWeightedPolicyAckDraftStoreFindsWhatItSaved();
    }
}
