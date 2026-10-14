namespace Acme.Workforce.Compliance.Tests.Attestation.Estimate;

using Acme.Workforce.Compliance.Attestation.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class AttestationEntryStoreChecks
{
    public static void MemoryAttestationEntryStoreFindsWhatItSaved()
    {
        var store = AttestationEntryStore.NewMemoryAttestationEntryStore();
        var item = AttestationEntry.NewAttestationEntry("code-882", "label-308");
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
        MemoryAttestationEntryStoreFindsWhatItSaved();
    }
}
