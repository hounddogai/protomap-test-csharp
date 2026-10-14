namespace Acme.Workforce.Orgdesign.Tests.Scenario.Estimate;

using Acme.Workforce.Orgdesign.Scenario.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class ManualScenarioEstimateStoreChecks
{
    public static void MemoryManualScenarioEstimateStoreFindsWhatItSaved()
    {
        var store = ManualScenarioEstimateStore.NewMemoryManualScenarioEstimateStore();
        var item = ManualScenarioEstimate.NewManualScenarioEstimate("code-750", "name-890");
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
        MemoryManualScenarioEstimateStoreFindsWhatItSaved();
    }
}
