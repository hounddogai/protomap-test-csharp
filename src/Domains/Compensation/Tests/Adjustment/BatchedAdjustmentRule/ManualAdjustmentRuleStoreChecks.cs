namespace Acme.Workforce.Compensation.Tests.Adjustment.BatchedAdjustmentRule;

using Acme.Workforce.Compensation.Adjustment.BatchedAdjustmentRule;

/// <summary>Checks of batched adjustment rule code, run by the project's runner.</summary>
public static class ManualAdjustmentRuleStoreChecks
{
    public static void MemoryManualAdjustmentRuleStoreFindsWhatItSaved()
    {
        var store = ManualAdjustmentRuleStore.NewMemoryManualAdjustmentRuleStore();
        var item = ManualAdjustmentRule.NewManualAdjustmentRule("code-770", "title-367");
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
        MemoryManualAdjustmentRuleStoreFindsWhatItSaved();
    }
}
