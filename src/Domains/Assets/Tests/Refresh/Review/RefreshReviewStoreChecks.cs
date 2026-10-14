namespace Acme.Workforce.Assets.Tests.Refresh.Review;

using Acme.Workforce.Assets.Refresh.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RefreshReviewStoreChecks
{
    public static void MemoryRefreshReviewStoreFindsWhatItSaved()
    {
        var store = RefreshReviewStore.NewMemoryRefreshReviewStore();
        var item = RefreshReview.NewRefreshReview("code-169", "title-231");
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
        MemoryRefreshReviewStoreFindsWhatItSaved();
    }
}
