namespace Acme.Workforce.Compliance.Tests.Attestation.Schedule;

using Acme.Workforce.Compliance.Attestation.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class DefaultAttestationClaimStoreChecks
{
    public static void MemoryDefaultAttestationClaimStoreFindsWhatItSaved()
    {
        var store = DefaultAttestationClaimStore.NewMemoryDefaultAttestationClaimStore();
        var item = DefaultAttestationClaim.NewDefaultAttestationClaim("code-884", "name-957");
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
        MemoryDefaultAttestationClaimStoreFindsWhatItSaved();
    }
}
