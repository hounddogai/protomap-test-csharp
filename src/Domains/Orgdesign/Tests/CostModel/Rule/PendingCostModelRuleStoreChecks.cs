namespace Acme.Workforce.Orgdesign.Tests.CostModel.Rule;

using Acme.Workforce.Orgdesign.CostModel.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class PendingCostModelRuleStoreChecks
{
    public static void MemoryPendingCostModelRuleStoreFindsWhatItSaved()
    {
        var store = PendingCostModelRuleStore.NewMemoryPendingCostModelRuleStore();
        var item = PendingCostModelRule.NewPendingCostModelRule("code-975", "label-985");
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
        MemoryPendingCostModelRuleStoreFindsWhatItSaved();
    }
}
