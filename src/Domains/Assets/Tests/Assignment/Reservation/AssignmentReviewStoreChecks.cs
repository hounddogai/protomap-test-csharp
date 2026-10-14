namespace Acme.Workforce.Assets.Tests.Assignment.Reservation;

using Acme.Workforce.Assets.Assignment.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class AssignmentReviewStoreChecks
{
    public static void MemoryAssignmentReviewStoreFindsWhatItSaved()
    {
        var store = AssignmentReviewStore.NewMemoryAssignmentReviewStore();
        var item = AssignmentReview.NewAssignmentReview("code-67", "title-385");
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
        MemoryAssignmentReviewStoreFindsWhatItSaved();
    }
}
