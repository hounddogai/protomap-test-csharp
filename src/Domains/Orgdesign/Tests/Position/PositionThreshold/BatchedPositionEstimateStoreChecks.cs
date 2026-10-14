namespace Acme.Workforce.Orgdesign.Tests.Position.PositionThreshold;

using Acme.Workforce.Orgdesign.Position.PositionThreshold;

/// <summary>Checks of position threshold code, run by the project's runner.</summary>
public static class BatchedPositionEstimateStoreChecks
{
    public static void MemoryBatchedPositionEstimateStoreFindsWhatItSaved()
    {
        var store = BatchedPositionEstimateStore.NewMemoryBatchedPositionEstimateStore();
        var item = BatchedPositionEstimate.NewBatchedPositionEstimate("code-742", "name-928");
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
        MemoryBatchedPositionEstimateStoreFindsWhatItSaved();
    }
}
