namespace Acme.Workforce.Orgdesign.Tests.Span.Review;

using Acme.Workforce.Orgdesign.Span.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RegionalSpanRecordStoreChecks
{
    public static void MemoryRegionalSpanRecordStoreFindsWhatItSaved()
    {
        var store = RegionalSpanRecordStore.NewMemoryRegionalSpanRecordStore();
        var item = RegionalSpanRecord.NewRegionalSpanRecord("code-687", "title-403");
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
        MemoryRegionalSpanRecordStoreFindsWhatItSaved();
    }
}
