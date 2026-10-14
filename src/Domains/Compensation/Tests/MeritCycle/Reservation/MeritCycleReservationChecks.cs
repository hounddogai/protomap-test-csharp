namespace Acme.Workforce.Compensation.Tests.MeritCycle.Reservation;

using Acme.Workforce.Compensation.MeritCycle.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class MeritCycleReservationChecks
{
    public static void MeritCycleReservationStartsInPlanned()
    {
        var item = MeritCycleReservation.NewMeritCycleReservation("code-574", "label-41");
        Check.Equal(item.Status, MeritCycleReservationStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MeritCycleReservationMovesToInProgress()
    {
        var item = MeritCycleReservation.NewMeritCycleReservation("code-574", "label-41");
        try
        {
            item.MoveTo(MeritCycleReservationStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MeritCycleReservationStatus.InProgress, "status");
    }

    public static void MeritCycleReservationRejectsMovesFromCancelled()
    {
        var item = MeritCycleReservation.NewMeritCycleReservation("code-574", "label-41");
        item.Status = MeritCycleReservationStatus.Cancelled;
        Check.Fails(() => item.MoveTo(MeritCycleReservationStatus.Planned), "move");
    }

    public static void RunAll()
    {
        MeritCycleReservationStartsInPlanned();
        MeritCycleReservationMovesToInProgress();
        MeritCycleReservationRejectsMovesFromCancelled();
    }
}
