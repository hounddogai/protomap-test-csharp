namespace Acme.Workforce.Workplace.Tests.Occupancy.Segment;

using Acme.Workforce.Workplace.Occupancy.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class OccupancySegmentStoreChecks
{
    public static void MemoryOccupancySegmentStoreFindsWhatItSaved()
    {
        var store = OccupancySegmentStore.NewMemoryOccupancySegmentStore();
        var item = OccupancySegment.NewOccupancySegment("code-732", "label-597");
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
        MemoryOccupancySegmentStoreFindsWhatItSaved();
    }
}
