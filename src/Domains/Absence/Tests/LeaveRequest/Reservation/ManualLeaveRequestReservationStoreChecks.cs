namespace Acme.Workforce.Absence.Tests.LeaveRequest.Reservation;

using Acme.Workforce.Absence.LeaveRequest.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class ManualLeaveRequestReservationStoreChecks
{
    public static void MemoryManualLeaveRequestReservationStoreFindsWhatItSaved()
    {
        var store = ManualLeaveRequestReservationStore.NewMemoryManualLeaveRequestReservationStore();
        var item = ManualLeaveRequestReservation.NewManualLeaveRequestReservation("code-116", "title-717");
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
        MemoryManualLeaveRequestReservationStoreFindsWhatItSaved();
    }
}
