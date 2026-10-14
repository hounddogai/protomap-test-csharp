namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioPolicy;

using Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

/// <summary>Checks of scenario policy code, run by the project's runner.</summary>
public static class ScenarioScoreStoreChecks
{
    public static void MemoryScenarioScoreStoreFindsWhatItSaved()
    {
        var store = ScenarioScoreStore.NewMemoryScenarioScoreStore();
        var item = ScenarioScore.NewScenarioScore("code-437", "name-680");
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
        MemoryScenarioScoreStoreFindsWhatItSaved();
    }
}
