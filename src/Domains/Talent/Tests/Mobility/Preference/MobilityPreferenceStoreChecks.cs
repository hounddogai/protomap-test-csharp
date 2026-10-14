namespace Acme.Workforce.Talent.Tests.Mobility.Preference;

using Acme.Workforce.Talent.Mobility.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class MobilityPreferenceStoreChecks
{
    public static void MemoryMobilityPreferenceStoreFindsWhatItSaved()
    {
        var store = MobilityPreferenceStore.NewMemoryMobilityPreferenceStore();
        var item = MobilityPreference.NewMobilityPreference("code-771", "title-41");
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
        MemoryMobilityPreferenceStoreFindsWhatItSaved();
    }
}
