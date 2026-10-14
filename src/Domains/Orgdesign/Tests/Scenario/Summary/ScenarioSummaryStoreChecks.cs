namespace Acme.Workforce.Orgdesign.Tests.Scenario.Summary;

using Acme.Workforce.Orgdesign.Scenario.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class ScenarioSummaryStoreChecks
{
    public static void MemoryScenarioSummaryStoreFindsWhatItSaved()
    {
        var store = ScenarioSummaryStore.NewMemoryScenarioSummaryStore();
        var item = ScenarioSummary.NewScenarioSummary("code-357", "title-208");
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
        MemoryScenarioSummaryStoreFindsWhatItSaved();
    }
}
