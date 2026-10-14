namespace Acme.Workforce.Compensation.Tests.PayBand.Reservation;

using Acme.Workforce.Compensation.PayBand.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class DefaultPayBandReservationChecks
{
    public static void DefaultPayBandReservationStartsInRequested()
    {
        var item = DefaultPayBandReservation.NewDefaultPayBandReservation("code-140", "title-71");
        Check.Equal(item.Status, DefaultPayBandReservationStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultPayBandReservationMovesToExpired()
    {
        var item = DefaultPayBandReservation.NewDefaultPayBandReservation("code-140", "title-71");
        try
        {
            item.MoveTo(DefaultPayBandReservationStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultPayBandReservationStatus.Expired, "status");
    }

    public static void DefaultPayBandReservationRejectsMovesFromExpired()
    {
        var item = DefaultPayBandReservation.NewDefaultPayBandReservation("code-140", "title-71");
        item.Status = DefaultPayBandReservationStatus.Expired;
        Check.Fails(() => item.MoveTo(DefaultPayBandReservationStatus.Requested), "move");
    }

    public static void RunAll()
    {
        DefaultPayBandReservationStartsInRequested();
        DefaultPayBandReservationMovesToExpired();
        DefaultPayBandReservationRejectsMovesFromExpired();
    }
}
