namespace Acme.Workforce.Workplace.Tests.Occupancy.Score;

using Acme.Workforce.Workplace.Occupancy.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class OccupancyScheduleStoreChecks
{
    public static void MemoryOccupancyScheduleStoreFindsWhatItSaved()
    {
        var store = OccupancyScheduleStore.NewMemoryOccupancyScheduleStore();
        var item = OccupancySchedule.NewOccupancySchedule("code-646", "title-947");
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
        MemoryOccupancyScheduleStoreFindsWhatItSaved();
    }
}
