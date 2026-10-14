namespace Acme.Workforce.Compliance.Tests.Attestation.Digest;

using Acme.Workforce.Compliance.Attestation.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class AttestationSnapshotStoreChecks
{
    public static void MemoryAttestationSnapshotStoreFindsWhatItSaved()
    {
        var store = AttestationSnapshotStore.NewMemoryAttestationSnapshotStore();
        var item = AttestationSnapshot.NewAttestationSnapshot("code-423", "label-98");
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
        MemoryAttestationSnapshotStoreFindsWhatItSaved();
    }
}
