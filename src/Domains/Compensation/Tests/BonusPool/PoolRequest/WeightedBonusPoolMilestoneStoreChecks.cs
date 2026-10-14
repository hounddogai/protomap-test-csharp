namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolRequest;

using Acme.Workforce.Compensation.BonusPool.PoolRequest;

/// <summary>Checks of pool request code, run by the project's runner.</summary>
public static class WeightedBonusPoolMilestoneStoreChecks
{
    public static void MemoryWeightedBonusPoolMilestoneStoreFindsWhatItSaved()
    {
        var store = WeightedBonusPoolMilestoneStore.NewMemoryWeightedBonusPoolMilestoneStore();
        var item = WeightedBonusPoolMilestone.NewWeightedBonusPoolMilestone("code-24", "label-525");
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
        MemoryWeightedBonusPoolMilestoneStoreFindsWhatItSaved();
    }
}
