namespace Acme.Workforce.Orgdesign.Tests.Span.SpanReview;

using Acme.Workforce.Orgdesign.Span.SpanReview;

/// <summary>Checks of span review code, run by the project's runner.</summary>
public static class RegionalSpanReviewStoreChecks
{
    public static void MemoryRegionalSpanReviewStoreFindsWhatItSaved()
    {
        var store = RegionalSpanReviewStore.NewMemoryRegionalSpanReviewStore();
        var item = RegionalSpanReview.NewRegionalSpanReview("code-185", "label-80");
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
        MemoryRegionalSpanReviewStoreFindsWhatItSaved();
    }
}
