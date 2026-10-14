namespace Acme.Workforce.Workplace.Tests.Occupancy.Batch;

using Acme.Workforce.Workplace.Occupancy.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class OccupancyNoticeStoreChecks
{
    public static void MemoryOccupancyNoticeStoreFindsWhatItSaved()
    {
        var store = OccupancyNoticeStore.NewMemoryOccupancyNoticeStore();
        var item = OccupancyNotice.NewOccupancyNotice("code-272", "name-358");
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
        MemoryOccupancyNoticeStoreFindsWhatItSaved();
    }
}
