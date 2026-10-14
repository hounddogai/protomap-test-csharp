namespace Acme.Workforce.Engagement.Tests.TownHall.Target;

using Acme.Workforce.Engagement.TownHall.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class SeasonalTownHallGroupStoreChecks
{
    public static void MemorySeasonalTownHallGroupStoreFindsWhatItSaved()
    {
        var store = SeasonalTownHallGroupStore.NewMemorySeasonalTownHallGroupStore();
        var item = SeasonalTownHallGroup.NewSeasonalTownHallGroup("code-574", "title-979");
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
        MemorySeasonalTownHallGroupStoreFindsWhatItSaved();
    }
}
