namespace Acme.Workforce.Compliance.Tests.PolicyAck.Claim;

using Acme.Workforce.Compliance.PolicyAck.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class BatchedPolicyAckSnapshotStoreChecks
{
    public static void MemoryBatchedPolicyAckSnapshotStoreFindsWhatItSaved()
    {
        var store = BatchedPolicyAckSnapshotStore.NewMemoryBatchedPolicyAckSnapshotStore();
        var item = BatchedPolicyAckSnapshot.NewBatchedPolicyAckSnapshot("code-656", "title-540");
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
        MemoryBatchedPolicyAckSnapshotStoreFindsWhatItSaved();
    }
}
