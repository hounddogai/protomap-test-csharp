namespace Acme.Workforce.Workplace.Tests.Occupancy.Plan;

using Acme.Workforce.Workplace.Occupancy.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class StrictOccupancyReviewStoreChecks
{
    public static void MemoryStrictOccupancyReviewStoreFindsWhatItSaved()
    {
        var store = StrictOccupancyReviewStore.NewMemoryStrictOccupancyReviewStore();
        var item = StrictOccupancyReview.NewStrictOccupancyReview("code-869", "title-229");
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
        MemoryStrictOccupancyReviewStoreFindsWhatItSaved();
    }
}
