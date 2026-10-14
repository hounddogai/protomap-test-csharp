namespace Acme.Workforce.Workplace.Tests.Desk.Preference;

using Acme.Workforce.Workplace.Desk.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class WeightedDeskPreferenceStoreChecks
{
    public static void MemoryWeightedDeskPreferenceStoreFindsWhatItSaved()
    {
        var store = WeightedDeskPreferenceStore.NewMemoryWeightedDeskPreferenceStore();
        var item = WeightedDeskPreference.NewWeightedDeskPreference("code-217", "label-343");
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
        MemoryWeightedDeskPreferenceStoreFindsWhatItSaved();
    }
}
