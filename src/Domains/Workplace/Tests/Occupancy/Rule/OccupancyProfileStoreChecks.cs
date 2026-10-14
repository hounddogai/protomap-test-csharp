namespace Acme.Workforce.Workplace.Tests.Occupancy.Rule;

using Acme.Workforce.Workplace.Occupancy.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class OccupancyProfileStoreChecks
{
    public static void MemoryOccupancyProfileStoreFindsWhatItSaved()
    {
        var store = OccupancyProfileStore.NewMemoryOccupancyProfileStore();
        var item = OccupancyProfile.NewOccupancyProfile("code-289", "title-856");
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
        MemoryOccupancyProfileStoreFindsWhatItSaved();
    }
}
