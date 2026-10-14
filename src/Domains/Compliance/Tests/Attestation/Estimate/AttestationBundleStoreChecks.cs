namespace Acme.Workforce.Compliance.Tests.Attestation.Estimate;

using Acme.Workforce.Compliance.Attestation.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class AttestationBundleStoreChecks
{
    public static void MemoryAttestationBundleStoreFindsWhatItSaved()
    {
        var store = AttestationBundleStore.NewMemoryAttestationBundleStore();
        var item = AttestationBundle.NewAttestationBundle("code-816", "name-823");
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
        MemoryAttestationBundleStoreFindsWhatItSaved();
    }
}
