namespace Acme.Workforce.Orgdesign.Tests.Span.Preference;

using Acme.Workforce.Orgdesign.Span.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class SpanRecordStoreChecks
{
    public static void MemorySpanRecordStoreFindsWhatItSaved()
    {
        var store = SpanRecordStore.NewMemorySpanRecordStore();
        var item = SpanRecord.NewSpanRecord("code-676", "label-557");
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
        MemorySpanRecordStoreFindsWhatItSaved();
    }
}
