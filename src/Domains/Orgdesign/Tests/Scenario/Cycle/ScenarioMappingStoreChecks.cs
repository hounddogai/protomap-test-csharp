namespace Acme.Workforce.Orgdesign.Tests.Scenario.Cycle;

using Acme.Workforce.Orgdesign.Scenario.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class ScenarioMappingStoreChecks
{
    public static void MemoryScenarioMappingStoreFindsWhatItSaved()
    {
        var store = ScenarioMappingStore.NewMemoryScenarioMappingStore();
        var item = ScenarioMapping.NewScenarioMapping("code-382", "name-157");
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
        MemoryScenarioMappingStoreFindsWhatItSaved();
    }
}
