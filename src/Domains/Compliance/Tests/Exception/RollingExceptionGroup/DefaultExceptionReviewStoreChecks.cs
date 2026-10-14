namespace Acme.Workforce.Compliance.Tests.Exception.RollingExceptionGroup;

using Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

/// <summary>Checks of rolling exception group code, run by the project's runner.</summary>
public static class DefaultExceptionReviewStoreChecks
{
    public static void MemoryDefaultExceptionReviewStoreFindsWhatItSaved()
    {
        var store = DefaultExceptionReviewStore.NewMemoryDefaultExceptionReviewStore();
        var item = DefaultExceptionReview.NewDefaultExceptionReview("code-815", "label-780");
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
        MemoryDefaultExceptionReviewStoreFindsWhatItSaved();
    }
}
