namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelManifest;

using Acme.Workforce.Orgdesign.CostModel.ModelManifest;

/// <summary>Checks of model manifest code, run by the project's runner.</summary>
public static class CappedCostModelRuleStoreChecks
{
    public static void MemoryCappedCostModelRuleStoreFindsWhatItSaved()
    {
        var store = CappedCostModelRuleStore.NewMemoryCappedCostModelRuleStore();
        var item = CappedCostModelRule.NewCappedCostModelRule("code-328", "name-873");
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
        MemoryCappedCostModelRuleStoreFindsWhatItSaved();
    }
}
