namespace Acme.Workforce.Orgdesign.Tests.Scenario.Group;

using Acme.Workforce.Orgdesign.Scenario.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ScenarioSlotStoreChecks
{
    public static void MemoryScenarioSlotStoreFindsWhatItSaved()
    {
        var store = ScenarioSlotStore.NewMemoryScenarioSlotStore();
        var item = ScenarioSlot.NewScenarioSlot("code-853", "name-763");
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
        MemoryScenarioSlotStoreFindsWhatItSaved();
    }
}
