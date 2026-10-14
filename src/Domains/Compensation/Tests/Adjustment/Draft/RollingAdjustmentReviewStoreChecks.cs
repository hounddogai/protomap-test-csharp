namespace Acme.Workforce.Compensation.Tests.Adjustment.Draft;

using Acme.Workforce.Compensation.Adjustment.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class RollingAdjustmentReviewStoreChecks
{
    public static void MemoryRollingAdjustmentReviewStoreFindsWhatItSaved()
    {
        var store = RollingAdjustmentReviewStore.NewMemoryRollingAdjustmentReviewStore();
        var item = RollingAdjustmentReview.NewRollingAdjustmentReview("code-55", "label-594");
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
        MemoryRollingAdjustmentReviewStoreFindsWhatItSaved();
    }
}
