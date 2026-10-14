namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioRule;

using Acme.Workforce.Orgdesign.Scenario.ScenarioRule;

/// <summary>Checks of scenario rule code, run by the project's runner.</summary>
public static class PrimaryScenarioSegmentStoreChecks
{
    public static void MemoryPrimaryScenarioSegmentStoreFindsWhatItSaved()
    {
        var store = PrimaryScenarioSegmentStore.NewMemoryPrimaryScenarioSegmentStore();
        var item = PrimaryScenarioSegment.NewPrimaryScenarioSegment("code-659", "title-698");
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
        MemoryPrimaryScenarioSegmentStoreFindsWhatItSaved();
    }
}
