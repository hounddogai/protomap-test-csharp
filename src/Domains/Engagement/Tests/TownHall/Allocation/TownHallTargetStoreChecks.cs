namespace Acme.Workforce.Engagement.Tests.TownHall.Allocation;

using Acme.Workforce.Engagement.TownHall.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class TownHallTargetStoreChecks
{
    public static void MemoryTownHallTargetStoreFindsWhatItSaved()
    {
        var store = TownHallTargetStore.NewMemoryTownHallTargetStore();
        var item = TownHallTarget.NewTownHallTarget("code-734", "title-560");
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
        MemoryTownHallTargetStoreFindsWhatItSaved();
    }
}
