namespace Acme.Workforce.Compliance.Tests.Attestation.Claim;

using Acme.Workforce.Compliance.Attestation.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AttestationClaimStoreChecks
{
    public static void MemoryAttestationClaimStoreFindsWhatItSaved()
    {
        var store = AttestationClaimStore.NewMemoryAttestationClaimStore();
        var item = AttestationClaim.NewAttestationClaim("code-611", "title-436");
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
        MemoryAttestationClaimStoreFindsWhatItSaved();
    }
}
