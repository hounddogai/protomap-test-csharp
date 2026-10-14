namespace Acme.Workforce.Engagement.Tests.TownHall.Milestone;

using Acme.Workforce.Engagement.TownHall.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class WeightedTownHallManifestStoreChecks
{
    public static void MemoryWeightedTownHallManifestStoreFindsWhatItSaved()
    {
        var store = WeightedTownHallManifestStore.NewMemoryWeightedTownHallManifestStore();
        var item = WeightedTownHallManifest.NewWeightedTownHallManifest("code-746", "name-118");
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
        MemoryWeightedTownHallManifestStoreFindsWhatItSaved();
    }
}
