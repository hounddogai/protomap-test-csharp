namespace Acme.Workforce.Orgdesign.Tests.Position.Plan;

using Acme.Workforce.Orgdesign.Position.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class PositionReviewStoreChecks
{
    public static void MemoryPositionReviewStoreFindsWhatItSaved()
    {
        var store = PositionReviewStore.NewMemoryPositionReviewStore();
        var item = PositionReview.NewPositionReview("code-634", "name-584");
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
        MemoryPositionReviewStoreFindsWhatItSaved();
    }
}
