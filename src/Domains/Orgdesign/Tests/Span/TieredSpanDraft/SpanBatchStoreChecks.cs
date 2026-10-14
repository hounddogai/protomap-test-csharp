namespace Acme.Workforce.Orgdesign.Tests.Span.TieredSpanDraft;

using Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

/// <summary>Checks of tiered span draft code, run by the project's runner.</summary>
public static class SpanBatchStoreChecks
{
    public static void MemorySpanBatchStoreFindsWhatItSaved()
    {
        var store = SpanBatchStore.NewMemorySpanBatchStore();
        var item = SpanBatch.NewSpanBatch("code-465", "label-815");
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
        MemorySpanBatchStoreFindsWhatItSaved();
    }
}
