namespace Acme.Workforce.Orgdesign.Tests.CostModel.Checkpoint;

using Acme.Workforce.Orgdesign.CostModel.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class AdaptiveCostModelCheckpointStoreChecks
{
    public static void MemoryAdaptiveCostModelCheckpointStoreFindsWhatItSaved()
    {
        var store = AdaptiveCostModelCheckpointStore.NewMemoryAdaptiveCostModelCheckpointStore();
        var item = AdaptiveCostModelCheckpoint.NewAdaptiveCostModelCheckpoint("code-80", "name-322");
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
        MemoryAdaptiveCostModelCheckpointStoreFindsWhatItSaved();
    }
}
