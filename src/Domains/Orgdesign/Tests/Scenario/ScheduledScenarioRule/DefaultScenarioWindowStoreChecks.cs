namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScheduledScenarioRule;

using Acme.Workforce.Orgdesign.Scenario.ScheduledScenarioRule;

/// <summary>Checks of scheduled scenario rule code, run by the project's runner.</summary>
public static class DefaultScenarioWindowStoreChecks
{
    public static void MemoryDefaultScenarioWindowStoreFindsWhatItSaved()
    {
        var store = DefaultScenarioWindowStore.NewMemoryDefaultScenarioWindowStore();
        var item = DefaultScenarioWindow.NewDefaultScenarioWindow("code-439", "label-870");
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
        MemoryDefaultScenarioWindowStoreFindsWhatItSaved();
    }
}
