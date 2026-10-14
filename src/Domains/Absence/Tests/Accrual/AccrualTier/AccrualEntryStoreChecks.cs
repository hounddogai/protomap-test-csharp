namespace Acme.Workforce.Absence.Tests.Accrual.AccrualTier;

using Acme.Workforce.Absence.Accrual.AccrualTier;

/// <summary>Checks of accrual tier code, run by the project's runner.</summary>
public static class AccrualEntryStoreChecks
{
    public static void MemoryAccrualEntryStoreFindsWhatItSaved()
    {
        var store = AccrualEntryStore.NewMemoryAccrualEntryStore();
        var item = AccrualEntry.NewAccrualEntry("code-68", "label-464");
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
        MemoryAccrualEntryStoreFindsWhatItSaved();
    }
}
