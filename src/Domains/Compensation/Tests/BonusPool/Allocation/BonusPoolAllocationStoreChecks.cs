namespace Acme.Workforce.Compensation.Tests.BonusPool.Allocation;

using Acme.Workforce.Compensation.BonusPool.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class BonusPoolAllocationStoreChecks
{
    public static void MemoryBonusPoolAllocationStoreFindsWhatItSaved()
    {
        var store = BonusPoolAllocationStore.NewMemoryBonusPoolAllocationStore();
        var item = BonusPoolAllocation.NewBonusPoolAllocation("code-496", "name-102");
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
        MemoryBonusPoolAllocationStoreFindsWhatItSaved();
    }
}
