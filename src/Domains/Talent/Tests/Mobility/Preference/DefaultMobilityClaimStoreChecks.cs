namespace Acme.Workforce.Talent.Tests.Mobility.Preference;

using Acme.Workforce.Talent.Mobility.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class DefaultMobilityClaimStoreChecks
{
    public static void MemoryDefaultMobilityClaimStoreFindsWhatItSaved()
    {
        var store = DefaultMobilityClaimStore.NewMemoryDefaultMobilityClaimStore();
        var item = DefaultMobilityClaim.NewDefaultMobilityClaim("code-628", "name-29");
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
        MemoryDefaultMobilityClaimStoreFindsWhatItSaved();
    }
}
