namespace Acme.Workforce.Compensation.Tests.BonusPool.Slot;

using Acme.Workforce.Compensation.BonusPool.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class BonusPoolSlotStoreChecks
{
    public static void MemoryBonusPoolSlotStoreFindsWhatItSaved()
    {
        var store = BonusPoolSlotStore.NewMemoryBonusPoolSlotStore();
        var item = BonusPoolSlot.NewBonusPoolSlot("code-881", "name-222");
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
        MemoryBonusPoolSlotStoreFindsWhatItSaved();
    }
}
