namespace Acme.Workforce.Engagement.Tests.TownHall.Draft;

using Acme.Workforce.Engagement.TownHall.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class TownHallBundleStoreChecks
{
    public static void MemoryTownHallBundleStoreFindsWhatItSaved()
    {
        var store = TownHallBundleStore.NewMemoryTownHallBundleStore();
        var item = TownHallBundle.NewTownHallBundle("code-452", "name-546");
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
        MemoryTownHallBundleStoreFindsWhatItSaved();
    }
}
