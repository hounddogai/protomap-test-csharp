namespace Acme.Workforce.Compensation.Tests.BonusPool.Batch;

using Acme.Workforce.Compensation.BonusPool.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class BonusPoolMilestoneStoreChecks
{
    public static void MemoryBonusPoolMilestoneStoreFindsWhatItSaved()
    {
        var store = BonusPoolMilestoneStore.NewMemoryBonusPoolMilestoneStore();
        var item = BonusPoolMilestone.NewBonusPoolMilestone("code-658", "title-294");
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
        MemoryBonusPoolMilestoneStoreFindsWhatItSaved();
    }
}
