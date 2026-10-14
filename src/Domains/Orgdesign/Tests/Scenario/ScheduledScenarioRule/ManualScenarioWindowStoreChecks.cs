namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScheduledScenarioRule;

using Acme.Workforce.Orgdesign.Scenario.ScheduledScenarioRule;

/// <summary>Checks of scheduled scenario rule code, run by the project's runner.</summary>
public static class ManualScenarioWindowStoreChecks
{
    public static void MemoryManualScenarioWindowStoreFindsWhatItSaved()
    {
        var store = ManualScenarioWindowStore.NewMemoryManualScenarioWindowStore();
        var item = ManualScenarioWindow.NewManualScenarioWindow("code-613", "title-689");
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
        MemoryManualScenarioWindowStoreFindsWhatItSaved();
    }
}
