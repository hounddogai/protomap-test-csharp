namespace Acme.Workforce.Engagement.Tests.TownHall.Cycle;

using Acme.Workforce.Engagement.TownHall.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class TownHallCycleStoreChecks
{
    public static void MemoryTownHallCycleStoreFindsWhatItSaved()
    {
        var store = TownHallCycleStore.NewMemoryTownHallCycleStore();
        var item = TownHallCycle.NewTownHallCycle("code-767", "label-537");
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
        MemoryTownHallCycleStoreFindsWhatItSaved();
    }
}
