namespace Acme.Workforce.Assets.Tests.Assignment.Reservation;

using Acme.Workforce.Assets.Assignment.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class GlobalAssignmentReservationStoreChecks
{
    public static void MemoryGlobalAssignmentReservationStoreFindsWhatItSaved()
    {
        var store = GlobalAssignmentReservationStore.NewMemoryGlobalAssignmentReservationStore();
        var item = GlobalAssignmentReservation.NewGlobalAssignmentReservation("code-389", "title-138");
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
        MemoryGlobalAssignmentReservationStoreFindsWhatItSaved();
    }
}
