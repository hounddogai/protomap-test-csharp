namespace Acme.Workforce.Compliance.Tests.Attestation.Digest;

using Acme.Workforce.Compliance.Attestation.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class AttestationDigestStoreChecks
{
    public static void MemoryAttestationDigestStoreFindsWhatItSaved()
    {
        var store = AttestationDigestStore.NewMemoryAttestationDigestStore();
        var item = AttestationDigest.NewAttestationDigest("code-429", "title-860");
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
        MemoryAttestationDigestStoreFindsWhatItSaved();
    }
}
