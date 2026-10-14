namespace Acme.Workforce.Orgdesign.Tests.Span.Review;

using Acme.Workforce.Orgdesign.Span.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RollingSpanReviewStoreChecks
{
    public static void MemoryRollingSpanReviewStoreFindsWhatItSaved()
    {
        var store = RollingSpanReviewStore.NewMemoryRollingSpanReviewStore();
        var item = RollingSpanReview.NewRollingSpanReview("code-342", "title-667");
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
        MemoryRollingSpanReviewStoreFindsWhatItSaved();
    }
}
