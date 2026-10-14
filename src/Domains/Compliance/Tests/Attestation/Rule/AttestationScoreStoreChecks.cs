namespace Acme.Workforce.Compliance.Tests.Attestation.Rule;

using Acme.Workforce.Compliance.Attestation.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AttestationScoreStoreChecks
{
    public static void MemoryAttestationScoreStoreFindsWhatItSaved()
    {
        var store = AttestationScoreStore.NewMemoryAttestationScoreStore();
        var item = AttestationScore.NewAttestationScore("code-144", "title-234");
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
        MemoryAttestationScoreStoreFindsWhatItSaved();
    }
}
