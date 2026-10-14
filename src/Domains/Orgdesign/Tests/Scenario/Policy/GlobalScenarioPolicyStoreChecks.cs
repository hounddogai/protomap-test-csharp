namespace Acme.Workforce.Orgdesign.Tests.Scenario.Policy;

using Acme.Workforce.Orgdesign.Scenario.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class GlobalScenarioPolicyStoreChecks
{
    public static void MemoryGlobalScenarioPolicyStoreFindsWhatItSaved()
    {
        var store = GlobalScenarioPolicyStore.NewMemoryGlobalScenarioPolicyStore();
        var item = GlobalScenarioPolicy.NewGlobalScenarioPolicy("code-231", "name-659");
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
        MemoryGlobalScenarioPolicyStoreFindsWhatItSaved();
    }
}
