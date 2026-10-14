namespace Acme.Workforce.Compensation.Tests.Adjustment.Segment;

using Acme.Workforce.Compensation.Adjustment.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AdjustmentCheckpointStoreChecks
{
    public static void MemoryAdjustmentCheckpointStoreFindsWhatItSaved()
    {
        var store = AdjustmentCheckpointStore.NewMemoryAdjustmentCheckpointStore();
        var item = AdjustmentCheckpoint.NewAdjustmentCheckpoint("code-317", "name-296");
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
        MemoryAdjustmentCheckpointStoreFindsWhatItSaved();
    }
}
