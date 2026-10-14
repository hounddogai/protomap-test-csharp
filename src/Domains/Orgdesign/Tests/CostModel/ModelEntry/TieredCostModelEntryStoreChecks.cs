namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelEntry;

using Acme.Workforce.Orgdesign.CostModel.ModelEntry;

/// <summary>Checks of model entry code, run by the project's runner.</summary>
public static class TieredCostModelEntryStoreChecks
{
    public static void MemoryTieredCostModelEntryStoreFindsWhatItSaved()
    {
        var store = TieredCostModelEntryStore.NewMemoryTieredCostModelEntryStore();
        var item = TieredCostModelEntry.NewTieredCostModelEntry("code-336", "name-76");
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
        MemoryTieredCostModelEntryStoreFindsWhatItSaved();
    }
}
