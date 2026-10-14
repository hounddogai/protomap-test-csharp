namespace Acme.Workforce.Orgdesign.Tests.Headcount.RecordItem;

using Acme.Workforce.Orgdesign.Headcount.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class HeadcountRecordStoreChecks
{
    public static void MemoryHeadcountRecordStoreFindsWhatItSaved()
    {
        var store = HeadcountRecordStore.NewMemoryHeadcountRecordStore();
        var item = HeadcountRecord.NewHeadcountRecord("code-849", "name-289");
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
        MemoryHeadcountRecordStoreFindsWhatItSaved();
    }
}
