namespace Acme.Workforce.Absence.Tests.LeaveRequest.Ledger;

using Acme.Workforce.Absence.LeaveRequest.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class SeasonalLeaveRequestReviewStoreChecks
{
    public static void MemorySeasonalLeaveRequestReviewStoreFindsWhatItSaved()
    {
        var store = SeasonalLeaveRequestReviewStore.NewMemorySeasonalLeaveRequestReviewStore();
        var item = SeasonalLeaveRequestReview.NewSeasonalLeaveRequestReview("code-573", "title-876");
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
        MemorySeasonalLeaveRequestReviewStoreFindsWhatItSaved();
    }
}
