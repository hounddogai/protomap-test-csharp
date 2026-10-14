namespace Acme.Workforce.Payrollops.Tests.Garnishment.Reservation;

using Acme.Workforce.Payrollops.Garnishment.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class GarnishmentReservationChecks
{
    public static void GarnishmentReservationStartsInPlanned()
    {
        var item = GarnishmentReservation.NewGarnishmentReservation("code-230", "label-375");
        Check.Equal(item.Status, GarnishmentReservationStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GarnishmentReservationMovesToCompleted()
    {
        var item = GarnishmentReservation.NewGarnishmentReservation("code-230", "label-375");
        try
        {
            item.MoveTo(GarnishmentReservationStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GarnishmentReservationStatus.Completed, "status");
    }

    public static void GarnishmentReservationRejectsMovesFromCancelled()
    {
        var item = GarnishmentReservation.NewGarnishmentReservation("code-230", "label-375");
        item.Status = GarnishmentReservationStatus.Cancelled;
        Check.Fails(() => item.MoveTo(GarnishmentReservationStatus.Planned), "move");
    }

    public static void RunAll()
    {
        GarnishmentReservationStartsInPlanned();
        GarnishmentReservationMovesToCompleted();
        GarnishmentReservationRejectsMovesFromCancelled();
    }
}
