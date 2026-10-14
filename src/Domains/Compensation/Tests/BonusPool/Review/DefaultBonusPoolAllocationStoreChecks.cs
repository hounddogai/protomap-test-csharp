namespace Acme.Workforce.Compensation.Tests.BonusPool.Review;

using Acme.Workforce.Compensation.BonusPool.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class DefaultBonusPoolAllocationStoreChecks
{
    public static void MemoryDefaultBonusPoolAllocationStoreFindsWhatItSaved()
    {
        var store = DefaultBonusPoolAllocationStore.NewMemoryDefaultBonusPoolAllocationStore();
        var item = DefaultBonusPoolAllocation.NewDefaultBonusPoolAllocation("code-62", "name-131");
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
        MemoryDefaultBonusPoolAllocationStoreFindsWhatItSaved();
    }
}
