namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioRule;

using Acme.Workforce.Orgdesign.Scenario.ScenarioRule;

/// <summary>Checks of scenario rule code, run by the project's runner.</summary>
public static class StrictScenarioRuleStoreChecks
{
    public static void MemoryStrictScenarioRuleStoreFindsWhatItSaved()
    {
        var store = StrictScenarioRuleStore.NewMemoryStrictScenarioRuleStore();
        var item = StrictScenarioRule.NewStrictScenarioRule("code-997", "label-909");
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
        MemoryStrictScenarioRuleStoreFindsWhatItSaved();
    }
}
