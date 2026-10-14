namespace Acme.Workforce.Orgdesign.Tests.CostModel.Preference;

using Acme.Workforce.Orgdesign.CostModel.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class CostModelBundleStoreChecks
{
    public static void MemoryCostModelBundleStoreFindsWhatItSaved()
    {
        var store = CostModelBundleStore.NewMemoryCostModelBundleStore();
        var item = CostModelBundle.NewCostModelBundle("code-814", "label-41");
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
        MemoryCostModelBundleStoreFindsWhatItSaved();
    }
}
