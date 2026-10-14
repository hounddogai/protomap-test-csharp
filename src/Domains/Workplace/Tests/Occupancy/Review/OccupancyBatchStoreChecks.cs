namespace Acme.Workforce.Workplace.Tests.Occupancy.Review;

using Acme.Workforce.Workplace.Occupancy.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class OccupancyBatchStoreChecks
{
    public static void MemoryOccupancyBatchStoreFindsWhatItSaved()
    {
        var store = OccupancyBatchStore.NewMemoryOccupancyBatchStore();
        var item = OccupancyBatch.NewOccupancyBatch("code-264", "label-146");
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
        MemoryOccupancyBatchStoreFindsWhatItSaved();
    }
}
