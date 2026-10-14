namespace Acme.Workforce.Absence.Tests.Accrual.Rollup;

using Acme.Workforce.Absence.Accrual.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class StrictAccrualSlotStoreChecks
{
    public static void MemoryStrictAccrualSlotStoreFindsWhatItSaved()
    {
        var store = StrictAccrualSlotStore.NewMemoryStrictAccrualSlotStore();
        var item = StrictAccrualSlot.NewStrictAccrualSlot("code-762", "label-673");
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
        MemoryStrictAccrualSlotStoreFindsWhatItSaved();
    }
}
