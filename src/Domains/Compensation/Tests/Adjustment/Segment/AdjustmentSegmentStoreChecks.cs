namespace Acme.Workforce.Compensation.Tests.Adjustment.Segment;

using Acme.Workforce.Compensation.Adjustment.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AdjustmentSegmentStoreChecks
{
    public static void MemoryAdjustmentSegmentStoreFindsWhatItSaved()
    {
        var store = AdjustmentSegmentStore.NewMemoryAdjustmentSegmentStore();
        var item = AdjustmentSegment.NewAdjustmentSegment("code-150", "label-84");
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
        MemoryAdjustmentSegmentStoreFindsWhatItSaved();
    }
}
