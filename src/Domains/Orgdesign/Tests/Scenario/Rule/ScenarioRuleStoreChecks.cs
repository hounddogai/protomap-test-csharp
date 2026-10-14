namespace Acme.Workforce.Orgdesign.Tests.Scenario.Rule;

using Acme.Workforce.Orgdesign.Scenario.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class ScenarioRuleStoreChecks
{
    public static void MemoryScenarioRuleStoreFindsWhatItSaved()
    {
        var store = ScenarioRuleStore.NewMemoryScenarioRuleStore();
        var item = ScenarioRule.NewScenarioRule("code-557", "label-384");
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
        MemoryScenarioRuleStoreFindsWhatItSaved();
    }
}
