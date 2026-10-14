namespace Acme.Workforce.Compliance.Tests.Attestation.Preference;

using Acme.Workforce.Compliance.Attestation.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RegionalAttestationPreferenceStoreChecks
{
    public static void MemoryRegionalAttestationPreferenceStoreFindsWhatItSaved()
    {
        var store = RegionalAttestationPreferenceStore.NewMemoryRegionalAttestationPreferenceStore();
        var item = RegionalAttestationPreference.NewRegionalAttestationPreference("code-194", "title-656");
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
        MemoryRegionalAttestationPreferenceStoreFindsWhatItSaved();
    }
}
