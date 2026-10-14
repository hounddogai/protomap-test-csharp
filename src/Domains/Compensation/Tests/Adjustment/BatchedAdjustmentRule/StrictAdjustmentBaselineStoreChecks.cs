namespace Acme.Workforce.Compensation.Tests.Adjustment.BatchedAdjustmentRule;

using Acme.Workforce.Compensation.Adjustment.BatchedAdjustmentRule;

/// <summary>Checks of batched adjustment rule code, run by the project's runner.</summary>
public static class StrictAdjustmentBaselineStoreChecks
{
    public static void MemoryStrictAdjustmentBaselineStoreFindsWhatItSaved()
    {
        var store = StrictAdjustmentBaselineStore.NewMemoryStrictAdjustmentBaselineStore();
        var item = StrictAdjustmentBaseline.NewStrictAdjustmentBaseline("code-658", "name-803");
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
        MemoryStrictAdjustmentBaselineStoreFindsWhatItSaved();
    }
}
