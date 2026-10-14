namespace Acme.Workforce.Orgdesign.Tests.CostModel.Preference;

using Acme.Workforce.Orgdesign.CostModel.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class SeasonalCostModelPreferenceStoreChecks
{
    public static void MemorySeasonalCostModelPreferenceStoreFindsWhatItSaved()
    {
        var store = SeasonalCostModelPreferenceStore.NewMemorySeasonalCostModelPreferenceStore();
        var item = SeasonalCostModelPreference.NewSeasonalCostModelPreference("code-88", "name-728");
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
        MemorySeasonalCostModelPreferenceStoreFindsWhatItSaved();
    }
}
