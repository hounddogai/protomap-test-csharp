namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelPreference;

using Acme.Workforce.Orgdesign.CostModel.ModelPreference;

/// <summary>Checks of model preference code, run by the project's runner.</summary>
public static class RollingCostModelPreferenceStoreChecks
{
    public static void MemoryRollingCostModelPreferenceStoreFindsWhatItSaved()
    {
        var store = RollingCostModelPreferenceStore.NewMemoryRollingCostModelPreferenceStore();
        var item = RollingCostModelPreference.NewRollingCostModelPreference("code-294", "title-981");
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
        MemoryRollingCostModelPreferenceStoreFindsWhatItSaved();
    }
}
