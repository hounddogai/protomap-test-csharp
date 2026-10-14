namespace Acme.Workforce.Talent.Tests.Mobility.Preference;

using Acme.Workforce.Talent.Mobility.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class SeasonalMobilityManifestStoreChecks
{
    public static void MemorySeasonalMobilityManifestStoreFindsWhatItSaved()
    {
        var store = SeasonalMobilityManifestStore.NewMemorySeasonalMobilityManifestStore();
        var item = SeasonalMobilityManifest.NewSeasonalMobilityManifest("code-112", "name-471");
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
        MemorySeasonalMobilityManifestStoreFindsWhatItSaved();
    }
}
