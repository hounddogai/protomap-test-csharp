namespace Acme.Workforce.Absence.Tests.Balance.RecordItem;

using Acme.Workforce.Absence.Balance.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class RollingBalanceRecordStoreChecks
{
    public static void MemoryRollingBalanceRecordStoreFindsWhatItSaved()
    {
        var store = RollingBalanceRecordStore.NewMemoryRollingBalanceRecordStore();
        var item = RollingBalanceRecord.NewRollingBalanceRecord("code-730", "label-509");
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
        MemoryRollingBalanceRecordStoreFindsWhatItSaved();
    }
}
