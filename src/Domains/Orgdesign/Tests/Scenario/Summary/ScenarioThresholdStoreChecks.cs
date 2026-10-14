namespace Acme.Workforce.Orgdesign.Tests.Scenario.Summary;

using Acme.Workforce.Orgdesign.Scenario.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class ScenarioThresholdStoreChecks
{
    public static void MemoryScenarioThresholdStoreFindsWhatItSaved()
    {
        var store = ScenarioThresholdStore.NewMemoryScenarioThresholdStore();
        var item = ScenarioThreshold.NewScenarioThreshold("code-922", "label-534");
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
        MemoryScenarioThresholdStoreFindsWhatItSaved();
    }
}
