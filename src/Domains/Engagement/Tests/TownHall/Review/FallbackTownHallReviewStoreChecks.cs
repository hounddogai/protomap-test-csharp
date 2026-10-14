namespace Acme.Workforce.Engagement.Tests.TownHall.Review;

using Acme.Workforce.Engagement.TownHall.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class FallbackTownHallReviewStoreChecks
{
    public static void MemoryFallbackTownHallReviewStoreFindsWhatItSaved()
    {
        var store = FallbackTownHallReviewStore.NewMemoryFallbackTownHallReviewStore();
        var item = FallbackTownHallReview.NewFallbackTownHallReview("code-596", "label-1");
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
        MemoryFallbackTownHallReviewStoreFindsWhatItSaved();
    }
}
