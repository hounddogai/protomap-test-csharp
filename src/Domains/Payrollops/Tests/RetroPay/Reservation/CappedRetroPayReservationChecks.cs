namespace Acme.Workforce.Payrollops.Tests.RetroPay.Reservation;

using Acme.Workforce.Payrollops.RetroPay.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class CappedRetroPayReservationChecks
{
    public static void CappedRetroPayReservationStartsInDraft()
    {
        var item = CappedRetroPayReservation.NewCappedRetroPayReservation("code-992", "name-369");
        Check.Equal(item.Status, CappedRetroPayReservationStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedRetroPayReservationMovesToActive()
    {
        var item = CappedRetroPayReservation.NewCappedRetroPayReservation("code-992", "name-369");
        try
        {
            item.MoveTo(CappedRetroPayReservationStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedRetroPayReservationStatus.Active, "status");
    }

    public static void CappedRetroPayReservationRejectsMovesFromClosed()
    {
        var item = CappedRetroPayReservation.NewCappedRetroPayReservation("code-992", "name-369");
        item.Status = CappedRetroPayReservationStatus.Closed;
        Check.Fails(() => item.MoveTo(CappedRetroPayReservationStatus.Draft), "move");
    }

    public static void RunAll()
    {
        CappedRetroPayReservationStartsInDraft();
        CappedRetroPayReservationMovesToActive();
        CappedRetroPayReservationRejectsMovesFromClosed();
    }
}
