namespace Acme.Workforce.Compliance.Tests.Attestation.Batch;

using Acme.Workforce.Compliance.Attestation.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class AttestationBatchStoreChecks
{
    public static void MemoryAttestationBatchStoreFindsWhatItSaved()
    {
        var store = AttestationBatchStore.NewMemoryAttestationBatchStore();
        var item = AttestationBatch.NewAttestationBatch("code-454", "name-300");
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
        MemoryAttestationBatchStoreFindsWhatItSaved();
    }
}
