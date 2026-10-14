namespace Acme.Workforce.Workplace.Tests.Desk.DeskRecord;

using Acme.Workforce.Workplace.Desk.DeskRecord;

/// <summary>Checks of desk record code, run by the project's runner.</summary>
public static class CappedDeskRecordStoreChecks
{
    public static void MemoryCappedDeskRecordStoreFindsWhatItSaved()
    {
        var store = CappedDeskRecordStore.NewMemoryCappedDeskRecordStore();
        var item = CappedDeskRecord.NewCappedDeskRecord("code-331", "name-917");
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
        MemoryCappedDeskRecordStoreFindsWhatItSaved();
    }
}
