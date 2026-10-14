namespace Acme.Workforce.Compliance.Tests.Attestation.Checkpoint;

using Acme.Workforce.Compliance.Attestation.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class GlobalAttestationCheckpointStoreChecks
{
    public static void MemoryGlobalAttestationCheckpointStoreFindsWhatItSaved()
    {
        var store = GlobalAttestationCheckpointStore.NewMemoryGlobalAttestationCheckpointStore();
        var item = GlobalAttestationCheckpoint.NewGlobalAttestationCheckpoint("code-291", "label-713");
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
        MemoryGlobalAttestationCheckpointStoreFindsWhatItSaved();
    }
}
