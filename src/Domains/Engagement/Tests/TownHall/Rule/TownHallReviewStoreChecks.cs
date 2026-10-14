namespace Acme.Workforce.Engagement.Tests.TownHall.Rule;

using Acme.Workforce.Engagement.TownHall.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class TownHallReviewStoreChecks
{
    public static void MemoryTownHallReviewStoreFindsWhatItSaved()
    {
        var store = TownHallReviewStore.NewMemoryTownHallReviewStore();
        var item = TownHallReview.NewTownHallReview("code-757", "label-498");
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
        MemoryTownHallReviewStoreFindsWhatItSaved();
    }
}
