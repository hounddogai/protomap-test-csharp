namespace Acme.Workforce.Compensation.Tests.Adjustment.Rule;

using Acme.Workforce.Compensation.Adjustment.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AdjustmentRuleStoreChecks
{
    public static void MemoryAdjustmentRuleStoreFindsWhatItSaved()
    {
        var store = AdjustmentRuleStore.NewMemoryAdjustmentRuleStore();
        var item = AdjustmentRule.NewAdjustmentRule("code-929", "label-810");
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
        MemoryAdjustmentRuleStoreFindsWhatItSaved();
    }
}
