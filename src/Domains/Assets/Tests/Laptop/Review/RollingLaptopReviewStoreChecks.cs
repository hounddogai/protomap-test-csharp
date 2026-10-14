namespace Acme.Workforce.Assets.Tests.Laptop.Review;

using Acme.Workforce.Assets.Laptop.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RollingLaptopReviewStoreChecks
{
    public static void MemoryRollingLaptopReviewStoreFindsWhatItSaved()
    {
        var store = RollingLaptopReviewStore.NewMemoryRollingLaptopReviewStore();
        var item = RollingLaptopReview.NewRollingLaptopReview("code-566", "label-618");
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
        MemoryRollingLaptopReviewStoreFindsWhatItSaved();
    }
}
