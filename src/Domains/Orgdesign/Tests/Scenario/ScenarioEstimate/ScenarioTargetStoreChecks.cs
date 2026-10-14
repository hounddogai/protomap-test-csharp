namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioEstimate;

using Acme.Workforce.Orgdesign.Scenario.ScenarioEstimate;

/// <summary>Checks of scenario estimate code, run by the project's runner.</summary>
public static class ScenarioTargetStoreChecks
{
    public static void MemoryScenarioTargetStoreFindsWhatItSaved()
    {
        var store = ScenarioTargetStore.NewMemoryScenarioTargetStore();
        var item = ScenarioTarget.NewScenarioTarget("code-924", "name-949");
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
        MemoryScenarioTargetStoreFindsWhatItSaved();
    }
}
