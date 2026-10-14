namespace Acme.Workforce.Workplace.Tests.Occupancy.Preference;

using Acme.Workforce.Workplace.Occupancy.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class OccupancyPolicyStoreChecks
{
    public static void MemoryOccupancyPolicyStoreFindsWhatItSaved()
    {
        var store = OccupancyPolicyStore.NewMemoryOccupancyPolicyStore();
        var item = OccupancyPolicy.NewOccupancyPolicy("code-296", "title-578");
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
        MemoryOccupancyPolicyStoreFindsWhatItSaved();
    }
}
