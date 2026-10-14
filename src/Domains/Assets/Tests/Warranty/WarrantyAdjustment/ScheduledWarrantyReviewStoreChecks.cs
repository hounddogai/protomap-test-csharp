namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyAdjustment;

using Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

/// <summary>Checks of warranty adjustment code, run by the project's runner.</summary>
public static class ScheduledWarrantyReviewStoreChecks
{
    public static void MemoryScheduledWarrantyReviewStoreFindsWhatItSaved()
    {
        var store = ScheduledWarrantyReviewStore.NewMemoryScheduledWarrantyReviewStore();
        var item = ScheduledWarrantyReview.NewScheduledWarrantyReview("code-452", "title-464");
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
        MemoryScheduledWarrantyReviewStoreFindsWhatItSaved();
    }
}
