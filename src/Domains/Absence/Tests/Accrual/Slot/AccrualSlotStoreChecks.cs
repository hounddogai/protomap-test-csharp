namespace Acme.Workforce.Absence.Tests.Accrual.Slot;

using Acme.Workforce.Absence.Accrual.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class AccrualSlotStoreChecks
{
    public static void MemoryAccrualSlotStoreFindsWhatItSaved()
    {
        var store = AccrualSlotStore.NewMemoryAccrualSlotStore();
        var item = AccrualSlot.NewAccrualSlot("code-110", "name-275");
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
        MemoryAccrualSlotStoreFindsWhatItSaved();
    }
}
