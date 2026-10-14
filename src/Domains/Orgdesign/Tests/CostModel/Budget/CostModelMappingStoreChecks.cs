namespace Acme.Workforce.Orgdesign.Tests.CostModel.Budget;

using Acme.Workforce.Orgdesign.CostModel.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class CostModelMappingStoreChecks
{
    public static void MemoryCostModelMappingStoreFindsWhatItSaved()
    {
        var store = CostModelMappingStore.NewMemoryCostModelMappingStore();
        var item = CostModelMapping.NewCostModelMapping("code-52", "name-606");
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
        MemoryCostModelMappingStoreFindsWhatItSaved();
    }
}
