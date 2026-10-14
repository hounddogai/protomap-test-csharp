namespace Acme.Workforce.Orgdesign.Tests.Scenario.Segment;

using Acme.Workforce.Orgdesign.Scenario.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class ScenarioSegmentStoreChecks
{
    public static void MemoryScenarioSegmentStoreFindsWhatItSaved()
    {
        var store = ScenarioSegmentStore.NewMemoryScenarioSegmentStore();
        var item = ScenarioSegment.NewScenarioSegment("code-632", "name-976");
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
        MemoryScenarioSegmentStoreFindsWhatItSaved();
    }
}
