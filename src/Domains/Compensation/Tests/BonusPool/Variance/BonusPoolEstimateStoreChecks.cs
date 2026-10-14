namespace Acme.Workforce.Compensation.Tests.BonusPool.Variance;

using Acme.Workforce.Compensation.BonusPool.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class BonusPoolEstimateStoreChecks
{
    public static void MemoryBonusPoolEstimateStoreFindsWhatItSaved()
    {
        var store = BonusPoolEstimateStore.NewMemoryBonusPoolEstimateStore();
        var item = BonusPoolEstimate.NewBonusPoolEstimate("code-378", "name-312");
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
        MemoryBonusPoolEstimateStoreFindsWhatItSaved();
    }
}
