namespace Acme.Workforce.Engagement.Tests.TownHall.Notice;

using Acme.Workforce.Engagement.TownHall.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class AdaptiveTownHallManifestStoreChecks
{
    public static void MemoryAdaptiveTownHallManifestStoreFindsWhatItSaved()
    {
        var store = AdaptiveTownHallManifestStore.NewMemoryAdaptiveTownHallManifestStore();
        var item = AdaptiveTownHallManifest.NewAdaptiveTownHallManifest("code-25", "name-668");
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
        MemoryAdaptiveTownHallManifestStoreFindsWhatItSaved();
    }
}
