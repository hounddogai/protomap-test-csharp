namespace Acme.Workforce.Orgdesign.Tests.CostModel.Summary;

using Acme.Workforce.Orgdesign.CostModel.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class StrictCostModelCheckpointStoreChecks
{
    public static void MemoryStrictCostModelCheckpointStoreFindsWhatItSaved()
    {
        var store = StrictCostModelCheckpointStore.NewMemoryStrictCostModelCheckpointStore();
        var item = StrictCostModelCheckpoint.NewStrictCostModelCheckpoint("code-760", "label-801");
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
        MemoryStrictCostModelCheckpointStoreFindsWhatItSaved();
    }
}
