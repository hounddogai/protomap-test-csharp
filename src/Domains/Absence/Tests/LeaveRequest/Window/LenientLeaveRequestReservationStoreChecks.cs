namespace Acme.Workforce.Absence.Tests.LeaveRequest.Window;

using Acme.Workforce.Absence.LeaveRequest.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class LenientLeaveRequestReservationStoreChecks
{
    public static void MemoryLenientLeaveRequestReservationStoreFindsWhatItSaved()
    {
        var store = LenientLeaveRequestReservationStore.NewMemoryLenientLeaveRequestReservationStore();
        var item = LenientLeaveRequestReservation.NewLenientLeaveRequestReservation("code-456", "title-612");
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
        MemoryLenientLeaveRequestReservationStoreFindsWhatItSaved();
    }
}
