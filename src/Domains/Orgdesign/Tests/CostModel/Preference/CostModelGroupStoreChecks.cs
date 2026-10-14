namespace Acme.Workforce.Orgdesign.Tests.CostModel.Preference;

using Acme.Workforce.Orgdesign.CostModel.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class CostModelGroupStoreChecks
{
    public static void MemoryCostModelGroupStoreFindsWhatItSaved()
    {
        var store = CostModelGroupStore.NewMemoryCostModelGroupStore();
        var item = CostModelGroup.NewCostModelGroup("code-404", "name-897");
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
        MemoryCostModelGroupStoreFindsWhatItSaved();
    }
}
