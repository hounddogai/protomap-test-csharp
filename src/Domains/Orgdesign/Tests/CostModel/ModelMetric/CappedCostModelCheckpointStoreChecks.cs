namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelMetric;

using Acme.Workforce.Orgdesign.CostModel.ModelMetric;

/// <summary>Checks of model metric code, run by the project's runner.</summary>
public static class CappedCostModelCheckpointStoreChecks
{
    public static void MemoryCappedCostModelCheckpointStoreFindsWhatItSaved()
    {
        var store = CappedCostModelCheckpointStore.NewMemoryCappedCostModelCheckpointStore();
        var item = CappedCostModelCheckpoint.NewCappedCostModelCheckpoint("code-561", "label-223");
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
        MemoryCappedCostModelCheckpointStoreFindsWhatItSaved();
    }
}
