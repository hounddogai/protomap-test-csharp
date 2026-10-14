namespace Acme.Workforce.Absence.Tests.LeaveRequest.Reservation;

using Acme.Workforce.Absence.LeaveRequest.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class ManualLeaveRequestReservationChecks
{
    public static void ManualLeaveRequestReservationStartsInPlanned()
    {
        var item = ManualLeaveRequestReservation.NewManualLeaveRequestReservation("code-116", "title-717");
        Check.Equal(item.Status, ManualLeaveRequestReservationStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualLeaveRequestReservationMovesToInProgress()
    {
        var item = ManualLeaveRequestReservation.NewManualLeaveRequestReservation("code-116", "title-717");
        try
        {
            item.MoveTo(ManualLeaveRequestReservationStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualLeaveRequestReservationStatus.InProgress, "status");
    }

    public static void ManualLeaveRequestReservationRejectsMovesFromCancelled()
    {
        var item = ManualLeaveRequestReservation.NewManualLeaveRequestReservation("code-116", "title-717");
        item.Status = ManualLeaveRequestReservationStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ManualLeaveRequestReservationStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ManualLeaveRequestReservationStartsInPlanned();
        ManualLeaveRequestReservationMovesToInProgress();
        ManualLeaveRequestReservationRejectsMovesFromCancelled();
    }
}
