namespace Acme.Workforce.Payrollops.Tests.PayRun.RunRecord;

using Acme.Workforce.Payrollops.PayRun.RunRecord;

/// <summary>Checks of run record code, run by the project's runner.</summary>
public static class CappedPayRunRecordStoreChecks
{
    public static void MemoryCappedPayRunRecordStoreFindsWhatItSaved()
    {
        var store = CappedPayRunRecordStore.NewMemoryCappedPayRunRecordStore();
        var item = CappedPayRunRecord.NewCappedPayRunRecord("code-986", "name-43");
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
        MemoryCappedPayRunRecordStoreFindsWhatItSaved();
    }
}
