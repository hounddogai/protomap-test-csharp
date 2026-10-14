namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioTemplate;

using Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

/// <summary>Checks of scenario template code, run by the project's runner.</summary>
public static class ManualScenarioAssignmentStoreChecks
{
    public static void MemoryManualScenarioAssignmentStoreFindsWhatItSaved()
    {
        var store = ManualScenarioAssignmentStore.NewMemoryManualScenarioAssignmentStore();
        var item = ManualScenarioAssignment.NewManualScenarioAssignment("code-442", "title-539");
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
        MemoryManualScenarioAssignmentStoreFindsWhatItSaved();
    }
}
