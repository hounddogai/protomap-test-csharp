namespace Acme.Workforce.Compliance.Tests.Attestation.Checkpoint;

using Acme.Workforce.Compliance.Attestation.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class AttestationRequestStoreChecks
{
    public static void MemoryAttestationRequestStoreFindsWhatItSaved()
    {
        var store = AttestationRequestStore.NewMemoryAttestationRequestStore();
        var item = AttestationRequest.NewAttestationRequest("code-870", "label-709");
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
        MemoryAttestationRequestStoreFindsWhatItSaved();
    }
}
