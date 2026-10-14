namespace Acme.Workforce.Absence.Tests.LeaveRequest.Window;

using Acme.Workforce.Absence.LeaveRequest.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class LenientLeaveRequestReservationChecks
{
    public static void LenientLeaveRequestReservationStartsInRequested()
    {
        var item = LenientLeaveRequestReservation.NewLenientLeaveRequestReservation("code-456", "title-612");
        Check.Equal(item.Status, LenientLeaveRequestReservationStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientLeaveRequestReservationMovesToGranted()
    {
        var item = LenientLeaveRequestReservation.NewLenientLeaveRequestReservation("code-456", "title-612");
        try
        {
            item.MoveTo(LenientLeaveRequestReservationStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientLeaveRequestReservationStatus.Granted, "status");
    }

    public static void LenientLeaveRequestReservationRejectsMovesFromExpired()
    {
        var item = LenientLeaveRequestReservation.NewLenientLeaveRequestReservation("code-456", "title-612");
        item.Status = LenientLeaveRequestReservationStatus.Expired;
        Check.Fails(() => item.MoveTo(LenientLeaveRequestReservationStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LenientLeaveRequestReservationStartsInRequested();
        LenientLeaveRequestReservationMovesToGranted();
        LenientLeaveRequestReservationRejectsMovesFromExpired();
    }
}
