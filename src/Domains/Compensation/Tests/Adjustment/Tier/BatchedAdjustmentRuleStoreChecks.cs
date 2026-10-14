namespace Acme.Workforce.Compensation.Tests.Adjustment.Tier;

using Acme.Workforce.Compensation.Adjustment.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class BatchedAdjustmentRuleStoreChecks
{
    public static void MemoryBatchedAdjustmentRuleStoreFindsWhatItSaved()
    {
        var store = BatchedAdjustmentRuleStore.NewMemoryBatchedAdjustmentRuleStore();
        var item = BatchedAdjustmentRule.NewBatchedAdjustmentRule("code-196", "title-555");
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
        MemoryBatchedAdjustmentRuleStoreFindsWhatItSaved();
    }
}
