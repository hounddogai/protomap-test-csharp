namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationSchedule;

using Acme.Workforce.Compliance.Attestation.AttestationSchedule;

/// <summary>Checks of attestation schedule code, run by the project's runner.</summary>
public static class PrimaryAttestationScoreStoreChecks
{
    public static void MemoryPrimaryAttestationScoreStoreFindsWhatItSaved()
    {
        var store = PrimaryAttestationScoreStore.NewMemoryPrimaryAttestationScoreStore();
        var item = PrimaryAttestationScore.NewPrimaryAttestationScore("code-217", "name-892");
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
        MemoryPrimaryAttestationScoreStoreFindsWhatItSaved();
    }
}
