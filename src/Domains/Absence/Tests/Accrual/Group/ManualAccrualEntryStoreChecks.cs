namespace Acme.Workforce.Absence.Tests.Accrual.Group;

using Acme.Workforce.Absence.Accrual.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ManualAccrualEntryStoreChecks
{
    public static void MemoryManualAccrualEntryStoreFindsWhatItSaved()
    {
        var store = ManualAccrualEntryStore.NewMemoryManualAccrualEntryStore();
        var item = ManualAccrualEntry.NewManualAccrualEntry("code-663", "label-971");
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
        MemoryManualAccrualEntryStoreFindsWhatItSaved();
    }
}
