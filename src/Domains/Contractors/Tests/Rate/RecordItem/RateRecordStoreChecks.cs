namespace Acme.Workforce.Contractors.Tests.Rate.RecordItem;

using Acme.Workforce.Contractors.Rate.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class RateRecordStoreChecks
{
    public static void MemoryRateRecordStoreFindsWhatItSaved()
    {
        var store = RateRecordStore.NewMemoryRateRecordStore();
        var item = RateRecord.NewRateRecord("code-196", "title-928");
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
        MemoryRateRecordStoreFindsWhatItSaved();
    }
}
