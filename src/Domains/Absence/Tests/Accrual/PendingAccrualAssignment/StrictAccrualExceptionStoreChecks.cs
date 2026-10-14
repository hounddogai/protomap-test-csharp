namespace Acme.Workforce.Absence.Tests.Accrual.PendingAccrualAssignment;

using Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;

/// <summary>Checks of pending accrual assignment code, run by the project's runner.</summary>
public static class StrictAccrualExceptionStoreChecks
{
    public static void MemoryStrictAccrualExceptionStoreFindsWhatItSaved()
    {
        var store = StrictAccrualExceptionStore.NewMemoryStrictAccrualExceptionStore();
        var item = StrictAccrualException.NewStrictAccrualException("code-951", "label-446");
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
        MemoryStrictAccrualExceptionStoreFindsWhatItSaved();
    }
}
