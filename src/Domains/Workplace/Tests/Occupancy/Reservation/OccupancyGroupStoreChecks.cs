namespace Acme.Workforce.Workplace.Tests.Occupancy.Reservation;

using Acme.Workforce.Workplace.Occupancy.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class OccupancyGroupStoreChecks
{
    public static void MemoryOccupancyGroupStoreFindsWhatItSaved()
    {
        var store = OccupancyGroupStore.NewMemoryOccupancyGroupStore();
        var item = OccupancyGroup.NewOccupancyGroup("code-615", "name-128");
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
        MemoryOccupancyGroupStoreFindsWhatItSaved();
    }
}
