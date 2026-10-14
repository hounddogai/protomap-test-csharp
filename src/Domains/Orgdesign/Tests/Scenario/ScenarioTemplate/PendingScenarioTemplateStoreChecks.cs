namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioTemplate;

using Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

/// <summary>Checks of scenario template code, run by the project's runner.</summary>
public static class PendingScenarioTemplateStoreChecks
{
    public static void MemoryPendingScenarioTemplateStoreFindsWhatItSaved()
    {
        var store = PendingScenarioTemplateStore.NewMemoryPendingScenarioTemplateStore();
        var item = PendingScenarioTemplate.NewPendingScenarioTemplate("code-847", "name-335");
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
        MemoryPendingScenarioTemplateStoreFindsWhatItSaved();
    }
}
