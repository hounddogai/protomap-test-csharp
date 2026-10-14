namespace Acme.Workforce.Orgdesign.Tests.Position.Segment;

using Acme.Workforce.Orgdesign.Position.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class BatchedPositionSegmentStoreChecks
{
    public static void MemoryBatchedPositionSegmentStoreFindsWhatItSaved()
    {
        var store = BatchedPositionSegmentStore.NewMemoryBatchedPositionSegmentStore();
        var item = BatchedPositionSegment.NewBatchedPositionSegment("code-246", "title-543");
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
        MemoryBatchedPositionSegmentStoreFindsWhatItSaved();
    }
}
