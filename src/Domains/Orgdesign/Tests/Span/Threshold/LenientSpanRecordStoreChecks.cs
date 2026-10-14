namespace Acme.Workforce.Orgdesign.Tests.Span.Threshold;

using Acme.Workforce.Orgdesign.Span.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LenientSpanRecordStoreChecks
{
    public static void MemoryLenientSpanRecordStoreFindsWhatItSaved()
    {
        var store = LenientSpanRecordStore.NewMemoryLenientSpanRecordStore();
        var item = LenientSpanRecord.NewLenientSpanRecord("code-724", "label-700");
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
        MemoryLenientSpanRecordStoreFindsWhatItSaved();
    }
}
