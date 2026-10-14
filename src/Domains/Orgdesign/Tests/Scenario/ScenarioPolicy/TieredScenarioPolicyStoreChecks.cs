namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioPolicy;

using Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

/// <summary>Checks of scenario policy code, run by the project's runner.</summary>
public static class TieredScenarioPolicyStoreChecks
{
    public static void MemoryTieredScenarioPolicyStoreFindsWhatItSaved()
    {
        var store = TieredScenarioPolicyStore.NewMemoryTieredScenarioPolicyStore();
        var item = TieredScenarioPolicy.NewTieredScenarioPolicy("code-485", "title-297");
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
        MemoryTieredScenarioPolicyStoreFindsWhatItSaved();
    }
}
