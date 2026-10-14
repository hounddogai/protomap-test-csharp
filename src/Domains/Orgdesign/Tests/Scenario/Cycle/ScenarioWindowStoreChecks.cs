namespace Acme.Workforce.Orgdesign.Tests.Scenario.Cycle;

using Acme.Workforce.Orgdesign.Scenario.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class ScenarioWindowStoreChecks
{
    public static void MemoryScenarioWindowStoreFindsWhatItSaved()
    {
        var store = ScenarioWindowStore.NewMemoryScenarioWindowStore();
        var item = ScenarioWindow.NewScenarioWindow("code-764", "label-111");
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
        MemoryScenarioWindowStoreFindsWhatItSaved();
    }
}
