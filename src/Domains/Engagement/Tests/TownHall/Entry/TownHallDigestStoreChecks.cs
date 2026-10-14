namespace Acme.Workforce.Engagement.Tests.TownHall.Entry;

using Acme.Workforce.Engagement.TownHall.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class TownHallDigestStoreChecks
{
    public static void MemoryTownHallDigestStoreFindsWhatItSaved()
    {
        var store = TownHallDigestStore.NewMemoryTownHallDigestStore();
        var item = TownHallDigest.NewTownHallDigest("code-57", "label-655");
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
        MemoryTownHallDigestStoreFindsWhatItSaved();
    }
}
