namespace Acme.Workforce.Compliance.Tests.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

/// <summary>Checks of seasonal exception group code, run by the project's runner.</summary>
public static class StrictExceptionRecordStoreChecks
{
    public static void MemoryStrictExceptionRecordStoreFindsWhatItSaved()
    {
        var store = StrictExceptionRecordStore.NewMemoryStrictExceptionRecordStore();
        var item = StrictExceptionRecord.NewStrictExceptionRecord("code-193", "title-247");
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
        MemoryStrictExceptionRecordStoreFindsWhatItSaved();
    }
}
