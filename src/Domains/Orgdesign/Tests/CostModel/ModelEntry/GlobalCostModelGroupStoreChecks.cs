namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelEntry;

using Acme.Workforce.Orgdesign.CostModel.ModelEntry;

/// <summary>Checks of model entry code, run by the project's runner.</summary>
public static class GlobalCostModelGroupStoreChecks
{
    public static void MemoryGlobalCostModelGroupStoreFindsWhatItSaved()
    {
        var store = GlobalCostModelGroupStore.NewMemoryGlobalCostModelGroupStore();
        var item = GlobalCostModelGroup.NewGlobalCostModelGroup("code-71", "name-271");
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
        MemoryGlobalCostModelGroupStoreFindsWhatItSaved();
    }
}
