namespace Acme.Workforce.Absence.Tests.LeaveRequest.Review;

using Acme.Workforce.Absence.LeaveRequest.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class StrictLeaveRequestReviewStoreChecks
{
    public static void MemoryStrictLeaveRequestReviewStoreFindsWhatItSaved()
    {
        var store = StrictLeaveRequestReviewStore.NewMemoryStrictLeaveRequestReviewStore();
        var item = StrictLeaveRequestReview.NewStrictLeaveRequestReview("code-718", "title-546");
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
        MemoryStrictLeaveRequestReviewStoreFindsWhatItSaved();
    }
}
