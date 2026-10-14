namespace Acme.Workforce.Engagement.Tests.TownHall.Adjustment;

using Acme.Workforce.Engagement.TownHall.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class SeasonalTownHallCycleStoreChecks
{
    public static void MemorySeasonalTownHallCycleStoreFindsWhatItSaved()
    {
        var store = SeasonalTownHallCycleStore.NewMemorySeasonalTownHallCycleStore();
        var item = SeasonalTownHallCycle.NewSeasonalTownHallCycle("code-321", "label-761");
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
        MemorySeasonalTownHallCycleStoreFindsWhatItSaved();
    }
}
