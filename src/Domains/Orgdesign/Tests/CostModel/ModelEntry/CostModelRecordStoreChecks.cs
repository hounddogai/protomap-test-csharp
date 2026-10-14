namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelEntry;

using Acme.Workforce.Orgdesign.CostModel.ModelEntry;

/// <summary>Checks of model entry code, run by the project's runner.</summary>
public static class CostModelRecordStoreChecks
{
    public static void MemoryCostModelRecordStoreFindsWhatItSaved()
    {
        var store = CostModelRecordStore.NewMemoryCostModelRecordStore();
        var item = CostModelRecord.NewCostModelRecord("code-549", "title-753");
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
        MemoryCostModelRecordStoreFindsWhatItSaved();
    }
}
