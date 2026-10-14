namespace Acme.Workforce.Workplace.Tests.Booking.BookingReservation;

using Acme.Workforce.Workplace.Booking.BookingReservation;

/// <summary>Checks of booking reservation code, run by the project's runner.</summary>
public static class RegionalBookingReservationChecks
{
    public static void RegionalBookingReservationStartsInOpen()
    {
        var item = RegionalBookingReservation.NewRegionalBookingReservation("code-680", "name-694");
        Check.Equal(item.Status, RegionalBookingReservationStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalBookingReservationMovesToInReview()
    {
        var item = RegionalBookingReservation.NewRegionalBookingReservation("code-680", "name-694");
        try
        {
            item.MoveTo(RegionalBookingReservationStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalBookingReservationStatus.InReview, "status");
    }

    public static void RegionalBookingReservationRejectsMovesFromArchived()
    {
        var item = RegionalBookingReservation.NewRegionalBookingReservation("code-680", "name-694");
        item.Status = RegionalBookingReservationStatus.Archived;
        Check.Fails(() => item.MoveTo(RegionalBookingReservationStatus.Open), "move");
    }

    public static void RunAll()
    {
        RegionalBookingReservationStartsInOpen();
        RegionalBookingReservationMovesToInReview();
        RegionalBookingReservationRejectsMovesFromArchived();
    }
}
