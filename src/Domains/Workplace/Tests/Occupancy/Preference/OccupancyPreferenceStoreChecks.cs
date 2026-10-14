namespace Acme.Workforce.Workplace.Tests.Occupancy.Preference;

using Acme.Workforce.Workplace.Occupancy.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class OccupancyPreferenceStoreChecks
{
    public static void MemoryOccupancyPreferenceStoreFindsWhatItSaved()
    {
        var store = OccupancyPreferenceStore.NewMemoryOccupancyPreferenceStore();
        var item = OccupancyPreference.NewOccupancyPreference("code-487", "title-703");
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
        MemoryOccupancyPreferenceStoreFindsWhatItSaved();
    }
}
