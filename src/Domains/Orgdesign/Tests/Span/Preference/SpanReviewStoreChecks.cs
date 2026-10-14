namespace Acme.Workforce.Orgdesign.Tests.Span.Preference;

using Acme.Workforce.Orgdesign.Span.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class SpanReviewStoreChecks
{
    public static void MemorySpanReviewStoreFindsWhatItSaved()
    {
        var store = SpanReviewStore.NewMemorySpanReviewStore();
        var item = SpanReview.NewSpanReview("code-354", "name-160");
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
        MemorySpanReviewStoreFindsWhatItSaved();
    }
}
