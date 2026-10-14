namespace Acme.Workforce.Engagement.Tests.TownHall.Threshold;

using Acme.Workforce.Engagement.TownHall.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LegacyTownHallCheckpointStoreChecks
{
    public static void MemoryLegacyTownHallCheckpointStoreFindsWhatItSaved()
    {
        var store = LegacyTownHallCheckpointStore.NewMemoryLegacyTownHallCheckpointStore();
        var item = LegacyTownHallCheckpoint.NewLegacyTownHallCheckpoint("code-168", "label-406");
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
        MemoryLegacyTownHallCheckpointStoreFindsWhatItSaved();
    }
}
