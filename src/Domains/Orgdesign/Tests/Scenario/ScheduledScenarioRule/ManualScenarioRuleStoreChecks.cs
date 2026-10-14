namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScheduledScenarioRule;

using Acme.Workforce.Orgdesign.Scenario.ScheduledScenarioRule;

/// <summary>Checks of scheduled scenario rule code, run by the project's runner.</summary>
public static class ManualScenarioRuleStoreChecks
{
    public static void MemoryManualScenarioRuleStoreFindsWhatItSaved()
    {
        var store = ManualScenarioRuleStore.NewMemoryManualScenarioRuleStore();
        var item = ManualScenarioRule.NewManualScenarioRule("code-542", "label-479");
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
        MemoryManualScenarioRuleStoreFindsWhatItSaved();
    }
}
