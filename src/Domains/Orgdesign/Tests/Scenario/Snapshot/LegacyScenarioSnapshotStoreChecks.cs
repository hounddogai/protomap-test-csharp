namespace Acme.Workforce.Orgdesign.Tests.Scenario.Snapshot;

using Acme.Workforce.Orgdesign.Scenario.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class LegacyScenarioSnapshotStoreChecks
{
    public static void MemoryLegacyScenarioSnapshotStoreFindsWhatItSaved()
    {
        var store = LegacyScenarioSnapshotStore.NewMemoryLegacyScenarioSnapshotStore();
        var item = LegacyScenarioSnapshot.NewLegacyScenarioSnapshot("code-202", "label-487");
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
        MemoryLegacyScenarioSnapshotStoreFindsWhatItSaved();
    }
}
