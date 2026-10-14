namespace Acme.Workforce.Orgdesign.Tests.Scenario.Segment;

using Acme.Workforce.Orgdesign.Scenario.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class ScenarioPolicyStoreChecks
{
    public static void MemoryScenarioPolicyStoreFindsWhatItSaved()
    {
        var store = ScenarioPolicyStore.NewMemoryScenarioPolicyStore();
        var item = ScenarioPolicy.NewScenarioPolicy("code-920", "name-821");
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
        MemoryScenarioPolicyStoreFindsWhatItSaved();
    }
}
