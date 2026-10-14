namespace Acme.Workforce.Engagement.Tests.TownHall.Milestone;

using Acme.Workforce.Engagement.TownHall.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class TownHallScheduleStoreChecks
{
    public static void MemoryTownHallScheduleStoreFindsWhatItSaved()
    {
        var store = TownHallScheduleStore.NewMemoryTownHallScheduleStore();
        var item = TownHallSchedule.NewTownHallSchedule("code-171", "label-781");
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
        MemoryTownHallScheduleStoreFindsWhatItSaved();
    }
}
