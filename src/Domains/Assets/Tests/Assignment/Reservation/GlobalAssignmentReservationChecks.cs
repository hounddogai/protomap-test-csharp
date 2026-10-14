namespace Acme.Workforce.Assets.Tests.Assignment.Reservation;

using Acme.Workforce.Assets.Assignment.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class GlobalAssignmentReservationChecks
{
    public static void GlobalAssignmentReservationStartsInPlanned()
    {
        var item = GlobalAssignmentReservation.NewGlobalAssignmentReservation("code-389", "title-138");
        Check.Equal(item.Status, GlobalAssignmentReservationStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalAssignmentReservationMovesToInProgress()
    {
        var item = GlobalAssignmentReservation.NewGlobalAssignmentReservation("code-389", "title-138");
        try
        {
            item.MoveTo(GlobalAssignmentReservationStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalAssignmentReservationStatus.InProgress, "status");
    }

    public static void GlobalAssignmentReservationRejectsMovesFromCancelled()
    {
        var item = GlobalAssignmentReservation.NewGlobalAssignmentReservation("code-389", "title-138");
        item.Status = GlobalAssignmentReservationStatus.Cancelled;
        Check.Fails(() => item.MoveTo(GlobalAssignmentReservationStatus.Planned), "move");
    }

    public static void RunAll()
    {
        GlobalAssignmentReservationStartsInPlanned();
        GlobalAssignmentReservationMovesToInProgress();
        GlobalAssignmentReservationRejectsMovesFromCancelled();
    }
}
