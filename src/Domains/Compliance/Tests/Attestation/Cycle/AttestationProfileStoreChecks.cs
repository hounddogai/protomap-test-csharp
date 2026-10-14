namespace Acme.Workforce.Compliance.Tests.Attestation.Cycle;

using Acme.Workforce.Compliance.Attestation.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class AttestationProfileStoreChecks
{
    public static void MemoryAttestationProfileStoreFindsWhatItSaved()
    {
        var store = AttestationProfileStore.NewMemoryAttestationProfileStore();
        var item = AttestationProfile.NewAttestationProfile("code-296", "label-743");
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
        MemoryAttestationProfileStoreFindsWhatItSaved();
    }
}
