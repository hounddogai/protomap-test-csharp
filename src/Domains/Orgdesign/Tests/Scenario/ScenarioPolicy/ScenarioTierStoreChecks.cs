namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioPolicy;

using Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

/// <summary>Checks of scenario policy code, run by the project's runner.</summary>
public static class ScenarioTierStoreChecks
{
    public static void MemoryScenarioTierStoreFindsWhatItSaved()
    {
        var store = ScenarioTierStore.NewMemoryScenarioTierStore();
        var item = ScenarioTier.NewScenarioTier("code-298", "name-825");
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
        MemoryScenarioTierStoreFindsWhatItSaved();
    }
}
