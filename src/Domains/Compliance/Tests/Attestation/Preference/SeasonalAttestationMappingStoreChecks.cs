namespace Acme.Workforce.Compliance.Tests.Attestation.Preference;

using Acme.Workforce.Compliance.Attestation.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class SeasonalAttestationMappingStoreChecks
{
    public static void MemorySeasonalAttestationMappingStoreFindsWhatItSaved()
    {
        var store = SeasonalAttestationMappingStore.NewMemorySeasonalAttestationMappingStore();
        var item = SeasonalAttestationMapping.NewSeasonalAttestationMapping("code-251", "name-121");
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
        MemorySeasonalAttestationMappingStoreFindsWhatItSaved();
    }
}
