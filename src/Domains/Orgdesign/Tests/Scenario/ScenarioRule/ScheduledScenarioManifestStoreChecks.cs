namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioRule;

using Acme.Workforce.Orgdesign.Scenario.ScenarioRule;

/// <summary>Checks of scenario rule code, run by the project's runner.</summary>
public static class ScheduledScenarioManifestStoreChecks
{
    public static void MemoryScheduledScenarioManifestStoreFindsWhatItSaved()
    {
        var store = ScheduledScenarioManifestStore.NewMemoryScheduledScenarioManifestStore();
        var item = ScheduledScenarioManifest.NewScheduledScenarioManifest("code-75", "name-699");
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
        MemoryScheduledScenarioManifestStoreFindsWhatItSaved();
    }
}
