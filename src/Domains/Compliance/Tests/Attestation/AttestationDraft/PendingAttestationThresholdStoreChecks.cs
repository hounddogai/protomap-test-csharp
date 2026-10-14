namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationDraft;

using Acme.Workforce.Compliance.Attestation.AttestationDraft;

/// <summary>Checks of attestation draft code, run by the project's runner.</summary>
public static class PendingAttestationThresholdStoreChecks
{
    public static void MemoryPendingAttestationThresholdStoreFindsWhatItSaved()
    {
        var store = PendingAttestationThresholdStore.NewMemoryPendingAttestationThresholdStore();
        var item = PendingAttestationThreshold.NewPendingAttestationThreshold("code-246", "label-184");
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
        MemoryPendingAttestationThresholdStoreFindsWhatItSaved();
    }
}
