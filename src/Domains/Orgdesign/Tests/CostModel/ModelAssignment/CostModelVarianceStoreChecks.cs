namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelAssignment;

using Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

/// <summary>Checks of model assignment code, run by the project's runner.</summary>
public static class CostModelVarianceStoreChecks
{
    public static void MemoryCostModelVarianceStoreFindsWhatItSaved()
    {
        var store = CostModelVarianceStore.NewMemoryCostModelVarianceStore();
        var item = CostModelVariance.NewCostModelVariance("code-213", "name-552");
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
        MemoryCostModelVarianceStoreFindsWhatItSaved();
    }
}
