namespace Acme.Workforce.Compliance.Tests.Exception.Review;

using Acme.Workforce.Compliance.Exception.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class ExceptionReviewStoreChecks
{
    public static void MemoryExceptionReviewStoreFindsWhatItSaved()
    {
        var store = ExceptionReviewStore.NewMemoryExceptionReviewStore();
        var item = ExceptionReview.NewExceptionReview("code-441", "name-735");
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
        MemoryExceptionReviewStoreFindsWhatItSaved();
    }
}
