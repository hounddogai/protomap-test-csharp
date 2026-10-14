namespace Acme.Workforce.Workplace.Tests.Desk.Entry;

using Acme.Workforce.Workplace.Desk.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class DeskReviewStoreChecks
{
    public static void MemoryDeskReviewStoreFindsWhatItSaved()
    {
        var store = DeskReviewStore.NewMemoryDeskReviewStore();
        var item = DeskReview.NewDeskReview("code-550", "name-414");
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
        MemoryDeskReviewStoreFindsWhatItSaved();
    }
}
