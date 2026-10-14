namespace Acme.Workforce.Compensation.Tests.BonusPool.Policy;

using Acme.Workforce.Compensation.BonusPool.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class BonusPoolCycleStoreChecks
{
    public static void MemoryBonusPoolCycleStoreFindsWhatItSaved()
    {
        var store = BonusPoolCycleStore.NewMemoryBonusPoolCycleStore();
        var item = BonusPoolCycle.NewBonusPoolCycle("code-989", "label-238");
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
        MemoryBonusPoolCycleStoreFindsWhatItSaved();
    }
}
