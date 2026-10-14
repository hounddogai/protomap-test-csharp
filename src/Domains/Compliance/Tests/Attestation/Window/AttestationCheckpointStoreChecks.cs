namespace Acme.Workforce.Compliance.Tests.Attestation.Window;

using Acme.Workforce.Compliance.Attestation.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class AttestationCheckpointStoreChecks
{
    public static void MemoryAttestationCheckpointStoreFindsWhatItSaved()
    {
        var store = AttestationCheckpointStore.NewMemoryAttestationCheckpointStore();
        var item = AttestationCheckpoint.NewAttestationCheckpoint("code-122", "label-697");
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
        MemoryAttestationCheckpointStoreFindsWhatItSaved();
    }
}
