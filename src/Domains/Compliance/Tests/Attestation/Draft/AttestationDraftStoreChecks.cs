namespace Acme.Workforce.Compliance.Tests.Attestation.Draft;

using Acme.Workforce.Compliance.Attestation.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class AttestationDraftStoreChecks
{
    public static void MemoryAttestationDraftStoreFindsWhatItSaved()
    {
        var store = AttestationDraftStore.NewMemoryAttestationDraftStore();
        var item = AttestationDraft.NewAttestationDraft("code-897", "label-47");
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
        MemoryAttestationDraftStoreFindsWhatItSaved();
    }
}
